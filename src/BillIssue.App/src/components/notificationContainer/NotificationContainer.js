import { useState, useEffect } from "react";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import NotificationService from "../../services/NotificationService";

const NotificationContainer = () => {
    const { showError } = useError();
 
    const [loading, setLoading] = useState(true);
    const [notifications, setNotifications] = useState([]);
    
    const loadNotificaitons = async () => {
        setLoading(true);
        const notificationResult = await NotificationService.getNotifications(showError);
        setNotifications(notificationResult);
        setLoading(false);
    };

    const handleNotificationDecision = async (id, decision) => {
        const notification = notifications.find(notification => notification.notificationId === id);

        if (await NotificationService.doNotificationDecision({
                notification,
                decision
            }, showError)) {
            //TODO reload session notifications
            window.location.reload();
        }
    }

    useEffect(() => {
        loadNotificaitons();
    }, []);

    return (
    <>
        <div className='content-container'>
            <div className='row control-row '>
                <div className='breadcrumbs col-sm-6'>
                    Notifications
                </div>
            </div>
            <hr className='col-sm-12 p-1'/>
            {loading ? <>LOADING</> : notifications?.length > 0 ? (
                <table className='col-sm-12 styled-table'>
                    <thead>
                        <tr>
                        <td>Notification text</td> 
                        <td>Actions</td>
                        </tr>
                    </thead>
                    <tbody>
                        {notifications.map((notification) => (
                            <tr>
                                <td>{notification.notificationText}</td>
                                <td>
                                    <button onClick={() => handleNotificationDecision(notification.notificationId, true)} type="submit" className="btn btn-success">Accept</button>
                                    <button onClick={() => handleNotificationDecision(notification.notificationId, false)} type="submit" className="btn btn-danger">Reject</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            ) : (
                <h4 className="text-center col-sm-12 p-5">No new notifications</h4>
            )}
        </div>
    </>
    )
}

export default NotificationContainer;