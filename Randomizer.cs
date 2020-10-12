using System;

namespace Leaf.xNet
{
    /// <summary>
    /// Класс-обёртка- для потокобезопасной генерации псевдослучайных чисел.
    /// Lazy-load singleton для ThreadStatic <see cref="Random"/>.
    /// </summary>
    public static class Randomizer
    {
        public static Random Instance => _rand ?? (_rand = new Random());
        [ThreadStatic] private static Random _rand;
    }
}
