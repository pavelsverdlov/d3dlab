using System;

namespace Veldrid {
    public static class Illegal {
        public static Exception Value<T>() {
            return new IllegalValueException<T>();
        }

        public class IllegalValueException<T> : VeldridException {
        }
    }
}
