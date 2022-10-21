using UnityEngine;
using System.Collections;

public class GoEffect : GoItem {
    [SerializeField] private float lifeTime = 3;

    private Coroutine deadRoutine;

    private GoPooler goPooler => UnityBean.BeanContainer.GetBean<GoPooler>();

    public virtual void Play() {
        deadRoutine = App.mainUI.Run(DieAfter(lifeTime));
        OnPlay();
    }

    public override void OnGoingIntoPool() {
        App.mainUI.Stop(deadRoutine);
    }

    protected virtual void OnPlay() {
    }

    private IEnumerator DieAfter(float wait) {
        yield return new WaitForSeconds(wait);
        goPooler.Return(this);
    }
}