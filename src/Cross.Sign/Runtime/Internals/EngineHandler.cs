using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cross.Core.Common.Logging;
using Cross.Core.Common.Model.Errors;
using Cross.Core.Common.Utils;
using Cross.Core.Models.Expirer;
using Cross.Core.Models.Relay;
using Cross.Core.Network.Models;
using Cross.Sign.Constants;
using Cross.Sign.Interfaces;
using Cross.Sign.Models;
using Cross.Sign.Models.Engine;
using Cross.Sign.Models.Engine.Events;
using Cross.Sign.Models.Engine.Methods;
using Cross.Sign.Utils;

namespace Cross.Sign
{
    public partial class Engine
    {
        async Task IEnginePrivate.OnSessionProposeRequest(string topic, JsonRpcRequest<SessionPropose> payload)
        {
            if (PrivateThis.ShouldIgnorePairingRequest(topic, payload.Method))
                return;
            
            var @params = payload.Params;
            var id = payload.Id;
            try
            {
                var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);
                var proposal = new ProposalStruct
                {
                    Id = id,
                    PairingTopic = topic,
                    Expiry = expiry,
                    Proposer = @params.Proposer,
                    Relays = @params.Relays,
                    RequiredNamespaces = @params.RequiredNamespaces,
                    OptionalNamespaces = @params.OptionalNamespaces,
                    SessionProperties = @params.SessionProperties
                };
                await PrivateThis.SetProposal(id, proposal);
                var hash = HashUtils.HashMessage(JsonConvert.SerializeObject(payload));
                var verifyContext = await VerifyContext(hash, proposal.Proposer.Metadata);
                SessionProposed?.Invoke(this, new SessionProposalEvent
                {
                    Id = id,
                    Proposal = proposal,
                    VerifiedContext = verifyContext
                });
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionPropose, SessionProposeResponseAutoReject>(id, topic,
                    Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionProposeResponse(string topic, JsonRpcResponse<SessionProposeResponse> payload)
        {
            var id = payload.Id;
            if (payload.IsError)
            {
                await Client.Proposal.Delete(id, Error.FromErrorType(ErrorType.USER_DISCONNECTED));
                SessionConnectionErrored?.Invoke(this, payload.Error.ToException());
            }
            else
            {
                var result = payload.Result;
                var proposal = Client.Proposal.Get(id);
                var selfPublicKey = proposal.Proposer.PublicKey;
                var peerPublicKey = result.ResponderPublicKey;

                var sessionTopic = await Client.CoreClient.Crypto.GenerateSharedKey(
                    selfPublicKey,
                    peerPublicKey
                );

                proposal.SessionTopic = sessionTopic;
                await Client.Proposal.Set(id, proposal);
                await Client.CoreClient.Pairing.Activate(topic);
 
                var attempts = 5;
                do
                {
                    try
                    {
                        _ = await Client.CoreClient.Relayer.Subscribe(sessionTopic);
                        return;
                    }
                    catch (Exception e)
                    {
                        CrossLogger.LogError($"Got error subscribing to topic, attempts left: {attempts}");
                        CrossLogger.LogError(e);
                        attempts--;
                        await Task.Yield();
                    }
                } while (attempts > 0);

                throw new IOException($"Could not subscribe to session topic {sessionTopic}");
            }
        }

        async Task IEnginePrivate.OnSessionSettleRequest(string topic, JsonRpcRequest<SessionSettle> payload)
        {
            var id = payload.Id;
            var @params = payload.Params;
            try
            {
                await PrivateThis.IsValidSessionSettleRequest(@params);

                var proposal = Array.Find(Client.Proposal.Values, p => p.SessionTopic == topic);

                var pairingTopic = proposal.PairingTopic;
                var relay = @params.Relay;
                var controller = @params.Controller;
                var expiry = @params.Expiry;
                var namespaces = @params.Namespaces;
                var sessionProperties = @params.SessionProperties;

                var session = new Session
                {
                    Topic = topic,
                    PairingTopic = pairingTopic,
                    Relay = relay,
                    Expiry = expiry,
                    Namespaces = namespaces,
                    Acknowledged = true,
                    Controller = controller.PublicKey,
                    Self = new Participant
                    {
                        Metadata = Client.Metadata,
                        PublicKey = ""
                    },
                    Peer = new Participant
                    {
                        PublicKey = controller.PublicKey,
                        Metadata = controller.Metadata
                    },
                    SessionProperties = sessionProperties,
#pragma warning disable S6602
                    RequiredNamespaces = proposal.RequiredNamespaces
#pragma warning restore S6602
                };
                await MessageHandler.SendResult<SessionSettle, bool>(payload.Id, topic, true);
                SessionConnected?.Invoke(this, session);
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionSettle, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionSettleResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            var session = Client.Session.Get(topic);
            if (payload.IsError)
            {
                var error = Error.FromErrorType(ErrorType.USER_DISCONNECTED);
                await Client.Session.Delete(topic, error);
                SessionRejected?.Invoke(this, session);

                // Still used do not remove
                _sessionEventsHandlerMap[$"session_approve{id}"](this, payload);
            }
            else
            {
                await Client.Session.Update(topic, new Session
                {
                    Acknowledged = true
                });
                SessionApproved?.Invoke(this, session);
                _sessionEventsHandlerMap[$"session_approve{id}"](this, payload);
            }
        }

        async Task IEnginePrivate.OnSessionUpdateRequest(string topic, JsonRpcRequest<SessionUpdate> payload)
        {
            var @params = payload.Params;
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidUpdate(topic, @params.Namespaces);

                await Client.Session.Update(topic, new Session
                {
                    Namespaces = @params.Namespaces
                });

                await MessageHandler.SendResult<SessionUpdate, bool>(id, topic, true);
                SessionUpdateRequest?.Invoke(this, new SessionUpdateEvent
                {
                    Id = id,
                    Topic = topic,
                    Params = @params
                });
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionUpdate, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionUpdateResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            SessionUpdated?.Invoke(this, new SessionEvent
            {
                Id = id,
                Topic = topic
            });
            // Still used, do not remove
            _sessionEventsHandlerMap[$"session_update{id}"](this, payload);
        }

        async Task IEnginePrivate.OnSessionExtendRequest(string topic, JsonRpcRequest<SessionExtend> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidExtend(topic);
                await PrivateThis.SetExpiry(topic, Clock.CalculateExpiry(SessionExpiry));
                await MessageHandler.SendResult<SessionExtend, bool>(id, topic, true);
                SessionExtendRequest?.Invoke(this, new SessionEvent
                {
                    Id = id,
                    Topic = topic
                });
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionExtend, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionExtendResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;
            SessionExtended?.Invoke(this, new SessionEvent
            {
                Topic = topic,
                Id = id
            });
            // Still used, do not remove
            _sessionEventsHandlerMap[$"session_extend{id}"](this, payload);
        }

        async Task IEnginePrivate.OnSessionPingRequest(string topic, JsonRpcRequest<SessionPing> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidPing(topic);
                await MessageHandler.SendResult<SessionPing, bool>(id, topic, true);
                SessionPinged?.Invoke(this, new SessionEvent
                {
                    Id = id,
                    Topic = topic
                });
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionPing, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionPingResponse(string topic, JsonRpcResponse<bool> payload)
        {
            var id = payload.Id;

            // put at the end of the stack to avoid a race condition
            // where session_ping listener is not yet initialized
            await Task.Delay(500);

            SessionPinged?.Invoke(this, new SessionEvent
            {
                Id = id,
                Topic = topic
            });

            // Still used, do not remove
            _sessionEventsHandlerMap[$"session_ping{id}"](this, payload);
        }

        async Task IEnginePrivate.OnSessionDeleteRequest(string topic, JsonRpcRequest<SessionDelete> payload)
        {
            var id = payload.Id;
            try
            {
                await PrivateThis.IsValidDisconnect(topic, payload.Params);

                await MessageHandler.SendResult<SessionDelete, bool>(id, topic, true);
                await PrivateThis.DeleteSession(topic);
                SessionDeleted?.Invoke(this, new SessionEvent
                {
                    Topic = topic,
                    Id = id
                });
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionDelete, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnSessionEventRequest(string topic, JsonRpcRequest<SessionEvent<JToken>> payload)
        {
            var @params = payload.Params;
            var id = payload.Id;
            try
            {
                var eventData = @params.Event;
                var eventName = eventData.Name;

                await IsValidSessionTopic(topic);

                _customSessionEventsHandlerMap[eventName]?.Invoke(this, @params);

                await MessageHandler.SendResult<SessionEvent<EventData<JToken>>, bool>(id, topic, true);
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionEvent<JToken>, bool>(id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnAuthenticateRequest(string topic, JsonRpcRequest<SessionAuthenticate> payload)
        {
            try
            {
                var id = payload.Id;
                var @params = payload.Params;

                var hash = HashUtils.HashMessage(JsonConvert.SerializeObject(payload));
                var verifyContext = await VerifyContext(hash, @params.Requester.Metadata);

                var request = new AuthPendingRequest
                {
                    Id = id,
                    PairingTopic = @params.Payload.PairingTopic,
                    Requester = @params.Requester,
                    PayloadParams = @params.Payload,
                    Expiry = @params.ExpiryTimestamp,
                    VerifyContext = verifyContext
                };

                @params.Payload.RequestId = id;

                await PrivateThis.SetAuthRequest(id, request);

                SessionAuthenticateRequest?.Invoke(this, @params);
            }
            catch (CrossNetworkException e)
            {
                await MessageHandler.SendError<SessionAuthenticate, SessionAuthenticateAutoReject>(payload.Id, topic, Error.FromException(e));
            }
        }

        async Task IEnginePrivate.OnAuthenticateResponse(string topic, JsonRpcResponse<AuthenticateResponse> payload)
        {
            // Delete this auth request on response
            // We're using payload from the wallet to establish the session so we don't need to keep this around
            await Client.Auth.PendingRequests.Delete(payload.Id, Error.FromErrorType(ErrorType.GENERIC));

            if (payload.IsError)
            {
                if (payload.Error.Code == Error.FromErrorType(ErrorType.WC_METHOD_UNSUPPORTED).Code)
                    return;

                throw CrossNetworkException.FromType((ErrorType)payload.Error.Code);
            }
            else
            {
                var cacaos = payload.Result.Cacaos;
                var responder = payload.Result.Responder;

                var approvedMethods = new HashSet<string>();
                var approvedAccounts = new HashSet<string>();
                foreach (var cacao in cacaos)
                {
                    var isValid = await cacao.VerifySignature(Client.CoreClient.ProjectId);
                    
                    if (!isValid)
                    {
                        UnityEngine.Debug.LogError($"[EngineHandler] CACAO signature verification failed!");
                        UnityEngine.Debug.LogError($"[EngineHandler] CACAO Domain: {cacao.Payload.Domain}");
                        UnityEngine.Debug.LogError($"[EngineHandler] CACAO Aud: {cacao.Payload.Aud}");
                        UnityEngine.Debug.LogError($"[EngineHandler] CACAO Iss: {cacao.Payload.Iss}");
                        throw new IOException("CACAO signature verification failed");
                    }

                    var approvedChains = new HashSet<string>();
                    var parsedAddress = CacaoUtils.ExtractDidAddress(cacao.Payload.Iss);

                    if (ReCap.TryGetRecapFromResources(cacao.Payload.Resources, out var recapStr))
                    {
                        var methodsFromRecap = ReCap.GetActionsFromEncodedRecap(recapStr);
                        var chainsFromRecap = ReCap.GetChainsFromEncodedRecap(recapStr);
                        
                        approvedMethods.UnionWith(methodsFromRecap);
                        approvedChains.UnionWith(chainsFromRecap);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[EngineHandler] No ReCap found in CACAO resources");
                    }

                    approvedAccounts.UnionWith(approvedChains.Select(chain => $"{chain}:{parsedAddress}"));
                }

                var publicKey = Client.Auth.Keys.Get(AuthConstants.AuthPublicKeyName).PublicKey;
                var sessionTopic = await Client.CoreClient.Crypto.GenerateSharedKey(publicKey, responder.PublicKey);

                Session session = default;

                if (approvedMethods.Count != 0)
                {
                    session = new Session
                    {
                        Topic = sessionTopic,
                        Acknowledged = true,
                        Self = new Participant
                        {
                            PublicKey = publicKey,
                            Metadata = Client.Metadata
                        },
                        Peer = responder,
                        Controller = responder.PublicKey,
                        Expiry = Clock.CalculateExpiry(SessionExpiry),
                        Namespaces = Namespaces.FromAuth(approvedMethods.ToArray(), approvedAccounts.ToArray()),
                        Relay = new ProtocolOptions
                        {
                            Protocol = "irn"
                        }
                    };
                    
                    await Client.CoreClient.Relayer.Subscribe(sessionTopic);
                    await Client.Session.Set(sessionTopic, session);

                    session = Client.Session.Get(sessionTopic);
                }
                else
                {
                    UnityEngine.Debug.LogError($"[EngineHandler] No approved methods found - session will not be created");
                }

                SessionAuthenticated?.Invoke(this, new SessionAuthenticatedEventArgs
                {
                    Session = session,
                    Auths = cacaos
                });
            }
        }

        private async void ExpiredCallback(object sender, ExpirerEventArgs e)
        {
            var target = new ExpirerTarget(e.Target);

            if (target.Id != null && Client.PendingRequests.Keys.Contains((long)target.Id))
            {
                await PrivateThis.DeletePendingSessionRequest((long)target.Id,
                    Error.FromErrorType(ErrorType.SESSION_REQUEST_EXPIRED), true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(target.Topic))
            {
                var topic = target.Topic;
                if (!Client.Session.Keys.Contains(topic))
                {
                    return;
                }

                var session = Client.Session.Get(topic);
                await PrivateThis.DeleteSession(topic);
                SessionExpired?.Invoke(this, session);
                SessionDeleted?.Invoke(this, new SessionEvent
                {
                    Topic = topic
                });
            }
            else if (target.Id != null)
            {
                await PrivateThis.DeleteProposal((long)target.Id);
            }
        }
    }
}