using System.Collections.Generic;
using System.Text;

namespace Leaf.xNet.Services.StormWall
{
    internal class StormWallSolver
    {
        private string _cE, _rpct;
        private int _cK;
        private int _rpctLastIndex;

        // Создает обратную коллекцию значение - ключ
        private readonly Dictionary<char, int> _rRpct = new Dictionary<char, int>();

        public void Init(string cE, int cK, string rpct)
        {
            _cE = cE;
            _cK = cK;
            _rpct = rpct;
            _rpctLastIndex = _rpct.Length - 1;

            // Gtb() \/
            _rRpct.Clear();
            for (int i = 0; i < _rpct.Length; i++)
                _rRpct[_rpct[i]] = i;
        }

        public string Solve() // Vgd() => c = cE, t = cK
        {
            int e = _cK;
            var n = new StringBuilder();

            foreach (var o in _cE)
            {
                n.Append(Csr(-1 * e, o));
                ++e;

                if (e > _rpctLastIndex)
                    e = 0;
            }

            return n.ToString();
        }

        private char Csr(int t, char c)
        {
            // r = _rpctLastIndex
            if (!_rRpct.ContainsKey(c))
                return c;

            int o = _rRpct[c] + t;
            if (o > _rpctLastIndex)
                o = o - _rpctLastIndex - 1;
            else if (0 > o)
                o = _rpctLastIndex + o + 1;

            return _rpct[o];
        }
    }
}
