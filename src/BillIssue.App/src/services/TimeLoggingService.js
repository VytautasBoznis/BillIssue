import { getUserSessionData } from "../utils/sessionUtils";
import { timeloggingClient } from "./clients/HttpClients";

const TimeLoggingService = {
  async logTimeEntry(timeEntry, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await timeloggingClient.LogTimeEntry(sessionData.authToken, timeEntry, onError);
      return response.timeLogEntryDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to save time entry");
    }
  },
  async editTimeEntry(timeEntry, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await timeloggingClient.EditTimeEntry(sessionData.authToken, timeEntry, onError);
      return response.timeLogEntryDto;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to edit time entry");
    }
  },
  async deleteTimeEntry(timeEntryId, onError) {
    try {
      const sessionData = getUserSessionData();
      await timeloggingClient.DeleteTimeEntry(sessionData.authToken, timeEntryId, onError);
      return true;
    } catch (error) {
      onError(error.response?.data?.message || "Failed to delete time entry");
    }
  },
  async loadWeeksTimeEntries(timeEntryLookupFilter, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await timeloggingClient.GetWeekOfTimeEntries(sessionData.authToken, timeEntryLookupFilter, onError);
      return response.timeLogEntriesForWeek;
    } catch (error) {
      onError(error.response?.data?.message || "Failed load time entries for week");
    }
  },
  async searchTimeLoggingEntries(searchTimeLoggingEntriesRequest, onError) {
    try {
      const sessionData = getUserSessionData();
      const response = await timeloggingClient.SearchTimeLogEntries(sessionData.authToken, searchTimeLoggingEntriesRequest, onError);
      return response.timeLogEntryDtos;
    } catch (error) {
      onError(error.response?.data?.message || "Failed searching for time entries");
    }
  },
}

export default TimeLoggingService;