using System;

namespace Cross.Core.Crypto.Encoder
{
    internal sealed class BaseX
    {
        private readonly char[] _alphabet;
        private readonly int _base;
        private readonly byte[] _baseMap = new byte[256];
        private readonly double _factor;
        private readonly double _iFactor;
        private readonly char _leader;
        private readonly string _name;

        public BaseX(string alphabet, string name)
        {
            if (alphabet.Length >= 255)
            {
                throw new ArgumentException("Alphabet too long", nameof(alphabet));
            }

            _name = name;
            _alphabet = alphabet.ToCharArray();

            for (var j = 0; j < _baseMap.Length; j++)
            {
                _baseMap[j] = 255;
            }

            for (var i = 0; i < alphabet.Length; i++)
            {
                var xc = alphabet[i];
                if (_baseMap[xc] != 255)
                    throw new ArgumentException(xc + " is ambiguous");
                _baseMap[xc] = (byte)i;
            }

            _base = alphabet.Length;
            _leader = alphabet[0];
            _factor = Math.Log(_base) / Math.Log(256);
            _iFactor = Math.Log(256) / Math.Log(_base);
        }

        public string Encode(byte[] source)
        {
            if (source.Length == 0)
            {
                return "";
            }

            // Skip & count leading zeroes.
            var zeroes = 0;
            var length = 0;
            var pbegin = 0;
            var pend = source.Length;
            while (pbegin != pend && source[pbegin] == 0)
            {
                pbegin++;
                zeroes++;
            }

            // Allocate enough space in big-endian base58 representation.
            var size = (uint)((pend - pbegin) * _iFactor + 1) >> 0;
            var b58 = new byte[size];
            // Process the bytes.
            while (pbegin != pend)
            {
                var carry = source[pbegin];
                // Apply "b58 = b58 * 256 + ch".
                var i = 0;
                for (var it1 = size - 1; (carry != 0 || i < length) && it1 != -1; it1--, i++)
                {
                    carry += (byte)((uint)(256 * b58[it1]) >> 0);
                    b58[it1] = (byte)((uint)(carry % _base) >> 0);
                    carry = (byte)((uint)(carry / _base) >> 0);
                }

                if (carry != 0)
                {
                    throw new InvalidOperationException("Non-zero carry");
                }

                length = i;
                pbegin++;
            }

            // Skip leading zeroes in base58 result.
            var it2 = size - length;
            while (it2 != size && b58[it2] == 0)
            {
                it2++;
            }

            // Translate the result into a string.
            var str = new string(_leader, zeroes);
            for (; it2 < size; ++it2)
            {
                str += _alphabet[b58[it2]];
            }

            return str;
        }

        public byte[] DecodeUnsafe(string source)
        {
            if (source.Length == 0)
                return Array.Empty<byte>();

            var psz = 0;
            // Skip leading spaces.
            if (source[psz] == ' ')
            {
                return null;
            }

            // Skip and count leading '1's.
            var zeroes = 0;
            var length = 0;
            while (psz < source.Length && source[psz] == _leader)
            {
                zeroes++;
                psz++;
            }

            // Allocate enough space in big-endian base256 representation.
            var size = (uint)((source.Length - psz) * _factor + 1) >> 0; // log(58) / log(256), rounded up.
            var b256 = new byte[size];
            // Process the characters.
            while (psz < source.Length && source[psz] > 0)
            {
                // Decode character
                var carry = _baseMap[source[psz]];
                // Invalid character
                if (carry == 255)
                {
                    return null;
                }

                var i = 0;
                for (var it3 = size - 1; (carry != 0 || i < length) && it3 != -1; it3--, i++)
                {
                    carry += (byte)((uint)(_base * b256[it3]) >> 0);
                    b256[it3] = (byte)((uint)(carry % 256) >> 0);
                    carry = (byte)((uint)(carry / 256) >> 0);
                }

                if (carry != 0)
                {
                    throw new InvalidOperationException("Non-zero carry");
                }

                length = i;
                psz++;
            }

            // Skip trailing spaces.
            if (psz < source.Length && source[psz] == ' ')
            {
                return null;
            }

            // Skip leading zeroes in b256.
            var it4 = size - length;
            while (it4 < b256.Length && it4 != size && b256[it4] == 0)
            {
                it4++;
            }

            var vch = new byte[zeroes + (size - it4)];
            var j = zeroes;
            while (it4 < b256.Length && j < vch.Length && it4 != size)
            {
                vch[j++] = b256[it4++];
            }

            return vch;
        }

        public byte[] Decode(string source)
        {
            var buffer = DecodeUnsafe(source);
            if (buffer != null)
                return buffer;
            throw new InvalidOperationException($"Non-{_name} character");
        }
    }
}