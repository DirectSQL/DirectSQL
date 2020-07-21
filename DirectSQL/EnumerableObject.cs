using System.Collections;
using System.Collections.Generic;

namespace DirectSQL{
    /// <summary>
    /// Walk around for prohibited implemeting IEnumerable<dynamic>
    /// </summary>
    public abstract class EnumerableObject<T>: IEnumerable<T> {
        public abstract IEnumerator<T> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }    
}