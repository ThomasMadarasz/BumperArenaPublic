using System;

namespace Utils.Runtime
{
    public static class Utility
    {
        #region Exposed

        public static Random m_rng
        {
            get
            {
                if (_rng == null) _rng = new Random();
                return _rng;
            }

            private set { _rng = value; }
        }

        #endregion


        #region Private

        private static Random _rng;

        #endregion
    }
}