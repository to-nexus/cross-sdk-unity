using UnityEngine;

namespace Cross.Sign.Unity.Utils
{
    public sealed class WaitForNthFrame : CustomYieldInstruction
    {
        private readonly int _framesToWait;

        public WaitForNthFrame(int n)
        {
            _framesToWait = n;
        }

        public override bool keepWaiting
        {
            get => Time.frameCount % _framesToWait != 0;
        }
    }
}