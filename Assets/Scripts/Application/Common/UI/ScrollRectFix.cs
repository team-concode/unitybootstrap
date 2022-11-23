using UnityEngine.UI;

public class ScrollRectFix : ScrollRect {
    protected override void LateUpdate() {
        var anchorPoint = content.anchoredPosition;
        base.LateUpdate();

        if (float.IsNaN(anchorPoint.x)) {
            anchorPoint.x = 0;
            horizontalNormalizedPosition = 0;
            content.anchoredPosition = anchorPoint;
            StopMovement();
        }
        
        if (float.IsNaN(anchorPoint.y)) {
            anchorPoint.y = 1;
            verticalNormalizedPosition = 0;
            content.anchoredPosition = anchorPoint;
            StopMovement();
        } 
    }
}
