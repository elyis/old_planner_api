namespace old_planner_api
{
    public static class Constants
    {
        public static readonly string serverUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";").First();

        public static readonly string localPathToStorages = @"/Resources/";
        public static readonly string localPathToProfileIcons = $"{localPathToStorages}ProfileIcons/";
        public static readonly string localPathToPrivateChatAttachments = $"{localPathToStorages}chat/private/attachments";
        public static readonly string localPathToChatIcons = $"{localPathToStorages}chat/icons";

        public static readonly string webPathToProfileIcons = $"{serverUrl}/api/upload/profileIcon/";

        public static readonly string webPathToChatPrivateAttachment = $"{serverUrl}/api/upload/chat/";


        public static readonly string webPathToPrivateChatIcons = $"{serverUrl}/api/upload/chatIcon/";
    }
}