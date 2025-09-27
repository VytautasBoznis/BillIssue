import { useState } from "react";
import Badge from 'react-bootstrap/Badge';
import XIcon from "../icons/XIcon";
import BellIcon from "../icons/BellIcon";
import MailIcon from "../icons/MailIcon";
import { getNotifications } from "../../utils/sessionUtils";
import './NotificationsOverlay.css';

const NotificationsOverlay = () => {

    const [ showNotifications, setShowNotifications ] = useState(false);
    const notifications = getNotifications();

    const handleNotificationToggleClick = () => {
      setShowNotifications(!showNotifications);
    }

    const handleNotificationClick = () => {
      window.location.href = "/notifications";
    }

    return (
        <>
        <div class="header-bell-container" onClick={() => handleNotificationToggleClick(true)}>
          <BellIcon/>
          {
            (notifications && notifications.length > 0) ?
            (<div class="header-notification">
              <Badge bg="secondary">{notifications.length}</Badge>
            </div>): (<></>) 
          }
        </div>
        {
          (showNotifications) ? (
            <div class="notification-container">
              <div class="notification-container-title-container d-flex">
                <div class="notification-container-title">Notifications</div>
                <div onClick={() => handleNotificationToggleClick()}><XIcon size={20} classProperty="notification-container-close"></XIcon></div>
              </div>
              <div class="notification-container-data">
                {notifications.length > 0 ? notifications.map(notification => {
                    return (
                    <div class="notification-content d-flex" onClick={() => handleNotificationClick()}>
                      <div class="notification-icon d-flex">
                        <MailIcon></MailIcon>
                      </div>
                      <div class="notification-text">{notification.notificationText}</div>
                      </div>
                    )
                  }) : (<div class="notification-content d-flex" onClick={() => handleNotificationClick()}>
                          <div class="notification-empty">No new notifications at this time</div>
                        </div>)}
              </div>
            </div>): (<></>)
        }</>
    )
};

export default NotificationsOverlay;