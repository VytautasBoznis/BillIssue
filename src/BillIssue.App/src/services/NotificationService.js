import { getUserSessionData } from "../utils/sessionUtils";
import { notificationsClient } from "./clients/HttpClients";

const NotificationService = {
    async getNotifications(onError) {
        try {
            const sessionData = getUserSessionData();
            const response = await notificationsClient.GetNotifications(sessionData.jwtToken, onError);
            return response.notificationDtos;
        } catch (error) {
            onError(error.response?.data?.message || "Failed to get notifications");
        }
    },
    async doNotificationDecision(doNotificationDecisionRequest, onError) {
        try {
            const sessionData = getUserSessionData();
            await notificationsClient.DoNotificationDecision(sessionData.jwtToken, doNotificationDecisionRequest, onError);
            return true;
        } catch (error) {
            onError(error.response?.data?.message || "Failed to get notifications");
        }
    }
}

export default NotificationService;