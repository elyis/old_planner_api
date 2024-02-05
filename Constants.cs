namespace old_planner_api
{
    public static class Constants
    {
        public static readonly string serverUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";").First();

        public static readonly string localPathToStorages = @"Resources/";
        public static readonly string localPathToProfileIcons = $"{localPathToStorages}ProfileIcons/";
        public static readonly string localPathToTaskChatAttachments = $"{localPathToStorages}chat/tasks/attachments";
        public static readonly string localPathToPrivateChatAttachments = $"{localPathToStorages}chat/private/attachments";

        public static readonly string webPathToProfileIcons = $"{serverUrl}/api/upload/profileIcon/";
        public static readonly string webPathToTaskChatAttachments = $"{serverUrl}/api/upload/chat/tasks/";
        public static readonly string webPathToChatPrivateAttachment = $"{serverUrl}/api/upload/chat/private/";
    }
}