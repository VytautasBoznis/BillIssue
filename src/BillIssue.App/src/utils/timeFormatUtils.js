export const formatTime = (timeInSeconds) => {
  if (!timeInSeconds) {
    return '00:00:00';
  }

  const hours = Math.floor((timeInSeconds / (60 * 60)) % 24);
  const minutes = Math.floor((timeInSeconds / 60) % 60);
  const seconds = Math.floor(timeInSeconds % 60);

  return hours.toString().padStart(2,'0') + ':' + minutes.toString().padStart(2,'0') + ':' + seconds.toString().padStart(2,'0');
}

export const formatSecondsToHoursAndMinutes = (timeInSeconds) => {
  if (!timeInSeconds) {
    return {
      hours: 0,
      minutes: 0
    }
  }

  return {
    hours: Math.floor((timeInSeconds / (60 * 60)) % 24),
    minutes: Math.floor((timeInSeconds / 60) % 60)
  }
}

export const getCurrentDateInISOFormat = () => {
  const currentDate = new Date();
  return currentDate.toJSON().substring(0, currentDate.toJSON().indexOf('T'));
}

export const getDisplayDateInISOFormat = (day) => {
  if(!day) return '';

  return day.substring(0, day.indexOf('T'));
}

export const getSecondsToHoursAndMinutesDisplay = (timeSpentSeconds) => {
  const formatedResult = formatSecondsToHoursAndMinutes(timeSpentSeconds);

  let result = `${formatedResult.hours}h`;

  if (formatedResult.minutes > 0) {
    result += ` ${formatedResult.minutes}m`; 
  }

  return result;
}

export const timestampToDate = (timestamp) => {
  const [year, month, day] = getDisplayDateInISOFormat(timestamp).split('-').map(Number);
  return new Date(year, month - 1, day);
}

export const dateToTimestamp = (date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}