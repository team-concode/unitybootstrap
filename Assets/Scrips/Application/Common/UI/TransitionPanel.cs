using System;
using UnityEngine;

public class TransitionPanel : MonoBehaviour {
    public virtual void Display(Action onDone) {
    }
    
    public virtual void Hide(bool easing,Action onDone) {
    }
}
