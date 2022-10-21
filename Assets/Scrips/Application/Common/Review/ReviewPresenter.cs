using System.Collections;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

public class ReviewPresenter {
    public static IEnumerator ShowReviewPopup() {
        var res = new AlertBoxOutResult();
        yield return App.mainUI.ShowAlertKey("review.2", AlertBoxType.Ok, res);
        
#if UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#elif UNITY_ANDROID
        var reviewManager = new ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError) {
            log.error(requestFlowOperation.Error.ToString());
            yield break;
        }

        var playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOperation;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError) {
            log.error(requestFlowOperation.Error.ToString());
            yield break;
        }
#endif
    }    
}
