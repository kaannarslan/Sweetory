using System;

namespace Sweetory.Core
{
    // Oyun verisi (JSON) okunamadığında veya geçersiz olduğunda fırlatılır.
    public class GameDataException : Exception
    {
        public GameDataException(string message, Exception inner = null) : base(message, inner) { }
    }
}
