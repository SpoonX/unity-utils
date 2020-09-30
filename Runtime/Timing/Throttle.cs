using UnityEngine;

namespace Spoonx.Timing
{
    public class Throttle
    {
        private float _timeBetweenUpdates;

        private float _timeSinceLastUpdate = float.PositiveInfinity;

        private int _ups;

        public bool Run(float updatesPerSecond)
        {
            if (_ups != updatesPerSecond)
            {
                _timeBetweenUpdates = 1f / updatesPerSecond;
                _ups = updatesPerSecond;
            }

            _timeSinceLastUpdate += Time.deltaTime;

            bool mayRun = _timeSinceLastUpdate >= _timeBetweenUpdates;

            if (mayRun) _timeSinceLastUpdate = 0;

            return mayRun;
        }
    }
}
