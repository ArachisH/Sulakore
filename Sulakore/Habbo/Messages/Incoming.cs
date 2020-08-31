﻿using System.Collections.Generic;

namespace Sulakore.Habbo.Messages
{
    public sealed class Incoming : HMessages
    {
        public HMessage Achievement { get; set; }
        public HMessage AchievementScore { get; set; }
        public HMessage AchievementUnlocked { get; set; }
        public HMessage Achievements { get; set; }
        public HMessage ActivityPointNotification { get; set; }
        public HMessage ActivityPoints { get; set; }
        public HMessage AddBot { get; set; }
        public HMessage AddPet { get; set; }
        public HMessage AuthenticationOK { get; set; }
        public HMessage AvailabilityStatus { get; set; }
        public HMessage AvailabilityTime { get; set; }
        public HMessage AvatarEffect { get; set; }
        public HMessage AvatarEffects { get; set; }
        public HMessage BadgePointLimits { get; set; }
        public HMessage BadgeReceived { get; set; }
        public HMessage BonusRareInfo { get; set; }
        public HMessage BotError { get; set; }
        public HMessage BotSettings { get; set; }
        public HMessage BuildersClubExpired { get; set; }
        public HMessage BuildersClubFurniCount { get; set; }
        public HMessage BuildersClubSubscriptionStatus { get; set; }
        public HMessage BullyReportClosed { get; set; }
        public HMessage BullyReportRequest { get; set; }
        public HMessage BullyReportedMessage { get; set; }
        public HMessage BundleDiscountRuleset { get; set; }
        public HMessage CameraCompetitionStatus { get; set; }
        public HMessage CameraPublishWaitMessage { get; set; }
        public HMessage CameraPurchaseSuccesful { get; set; }
        public HMessage CameraRoomThumbnailSaved { get; set; }
        public HMessage CameraStorageUrl { get; set; }
        public HMessage CampaignCalendarData { get; set; }
        public HMessage CampaignCalendarDoorOpened { get; set; }
        public HMessage CanCreateEvent { get; set; }
        public HMessage CanCreateRoom { get; set; }
        public HMessage CancelMysteryBoxWait { get; set; }
        public HMessage CantScratchPetNotOldEnough { get; set; }
        public HMessage CarryObject { get; set; }
        public HMessage CatalogIndex { get; set; }
        public HMessage CatalogMode { get; set; }
        public HMessage CatalogPage { get; set; }
        public HMessage CatalogPageExpiration { get; set; }
        public HMessage CatalogPageWithEarliestExpiry { get; set; }
        public HMessage CatalogPublished { get; set; }
        public HMessage CfhChatlog { get; set; }
        public HMessage CfhSanction { get; set; }
        public HMessage CfhTopicsInit { get; set; }
        public HMessage ChangeNameUpdate { get; set; }
        public HMessage Chat { get; set; }
        public HMessage CloseConnection { get; set; }
        public HMessage CloseWebPage { get; set; }
        public HMessage ClubCenterData { get; set; }
        public HMessage ClubData { get; set; }
        public HMessage ClubGiftInfo { get; set; }
        public HMessage ClubGiftNotification { get; set; }
        public HMessage ClubGiftSelected { get; set; }
        public HMessage CollapsedCategories { get; set; }
        public HMessage CompetitionEntrySubmitResult { get; set; }
        public HMessage CompleteDiffieHandshake { get; set; }
        public HMessage ConvertedRoomId { get; set; }
        public HMessage CraftableProducts { get; set; }
        public HMessage CraftingComposerFour { get; set; }
        public HMessage CraftingRecipe { get; set; }
        public HMessage CraftingResult { get; set; }
        public HMessage CreditBalance { get; set; }
        public HMessage CurrentTimingCode { get; set; }
        public HMessage CustomUserNotification { get; set; }
        public HMessage DailyQuest { get; set; }
        public HMessage Dance { get; set; }
        public HMessage DebugConsole { get; set; }
        public HMessage DiceValue { get; set; }
        public HMessage DirectSMSClubBuyAvailable { get; set; }
        public HMessage DisconnectReason { get; set; }
        public HMessage DoorbellAddUser { get; set; }
        public HMessage EffectsListAdd { get; set; }
        public HMessage EffectsListEffectEnable { get; set; }
        public HMessage EffectsListRemove { get; set; }
        public HMessage EmailStatus { get; set; }
        public HMessage EpicPopupFrame { get; set; }
        public HMessage ErrorReport { get; set; }
        public HMessage Expression { get; set; }
        public HMessage ExtendClubMessage { get; set; }
        public HMessage FavoriteRoomChanged { get; set; }
        public HMessage FavoriteRoomsCount { get; set; }
        public HMessage FigureSetIds { get; set; }
        public HMessage FlatAccessDenied { get; set; }
        public HMessage FlatAccessible { get; set; }
        public HMessage FlatCreated { get; set; }
        public HMessage FloodCounter { get; set; }
        public HMessage FloorHeightMap { get; set; }
        public HMessage FloorItemUpdate { get; set; }
        public HMessage FloorPlanEditorBlockedTiles { get; set; }
        public HMessage FloorPlanEditorDoorSettings { get; set; }
        public HMessage FollowFriendFailed { get; set; }
        public HMessage ForwardToRoom { get; set; }
        public HMessage FreezeLives { get; set; }
        public HMessage FriendChatMessage { get; set; }
        public HMessage FriendFindingRoom { get; set; }
        public HMessage FriendListUpdate { get; set; }
        public HMessage FriendRequest { get; set; }
        public HMessage FriendRequestError { get; set; }
        public HMessage FriendRequests { get; set; }
        public HMessage FriendToolbarNotification { get; set; }
        public HMessage Friends { get; set; }
        public HMessage FurniListAddOrUpdateParser { get; set; }
        public HMessage FurniListRemove { get; set; }
        public HMessage FurnitureAliases { get; set; }
        public HMessage Game2AccountGameStatus { get; set; }
        public HMessage Game2WeeklyLeaderboard { get; set; }
        public HMessage Game2WeeklySmallLeaderboard { get; set; }
        public HMessage GameAchievementsList { get; set; }
        public HMessage GameCenterFeaturedPlayers { get; set; }
        public HMessage GameCenterGame { get; set; }
        public HMessage GameCenterGameList { get; set; }
        public HMessage GenericAlert { get; set; }
        public HMessage GenericError { get; set; }
        public HMessage GenericErrorMessages { get; set; }
        public HMessage GetGuestRoomResult { get; set; }
        public HMessage GiftReceiverNotFound { get; set; }
        public HMessage GiftWrappingConfiguration { get; set; }
        public HMessage GotMysteryBoxPrize { get; set; }
        public HMessage GroupParts { get; set; }
        public HMessage GuardianNewReportReceived { get; set; }
        public HMessage GuardianVotingRequested { get; set; }
        public HMessage GuardianVotingResult { get; set; }
        public HMessage GuardianVotingTimeEnded { get; set; }
        public HMessage GuardianVotingVotes { get; set; }
        public HMessage GuideSessionAttached { get; set; }
        public HMessage GuideSessionDetached { get; set; }
        public HMessage GuideSessionEnded { get; set; }
        public HMessage GuideSessionError { get; set; }
        public HMessage GuideSessionInvitedToGuideRoom { get; set; }
        public HMessage GuideSessionMessage { get; set; }
        public HMessage GuideSessionPartnerIsTyping { get; set; }
        public HMessage GuideSessionRequesterRoom { get; set; }
        public HMessage GuideSessionStarted { get; set; }
        public HMessage GuideTools { get; set; }
        public HMessage GuildAcceptMemberError { get; set; }
        public HMessage GuildBought { get; set; }
        public HMessage GuildBuyRooms { get; set; }
        public HMessage GuildConfirmRemoveMember { get; set; }
        public HMessage GuildEditFail { get; set; }
        public HMessage GuildFavoriteRoomUserUpdate { get; set; }
        public HMessage GuildForumAddComment { get; set; }
        public HMessage GuildForumComments { get; set; }
        public HMessage GuildForumData { get; set; }
        public HMessage GuildForumList { get; set; }
        public HMessage GuildForumThreadMessages { get; set; }
        public HMessage GuildForumThreads { get; set; }
        public HMessage GuildForumsUnreadMessagesCount { get; set; }
        public HMessage GuildFurniWidget { get; set; }
        public HMessage GuildInfo { get; set; }
        public HMessage GuildJoinError { get; set; }
        public HMessage GuildList { get; set; }
        public HMessage GuildManage { get; set; }
        public HMessage GuildMemberUpdate { get; set; }
        public HMessage GuildMembers { get; set; }
        public HMessage GuildRefreshMembersList { get; set; }
        public HMessage HabboActivityPointNotification { get; set; }
        public HMessage HabboClubExtendOffer { get; set; }
        public HMessage HabboClubOffers { get; set; }
        public HMessage HabboGroupBadges { get; set; }
        public HMessage HallOfFame { get; set; }
        public HMessage HeightMap { get; set; }
        public HMessage HeightMapUpdate { get; set; }
        public HMessage HelperRequestDisabled { get; set; }
        public HMessage HotelViewBadgeButtonConfig { get; set; }
        public HMessage HotelViewCommunityGoal { get; set; }
        public HMessage HotelViewConcurrentUsers { get; set; }
        public HMessage HotelViewCustomTimer { get; set; }
        public HMessage HotelViewHideCommunityVoteButton { get; set; }
        public HMessage IdentityAccounts { get; set; }
        public HMessage IgnoredUsers { get; set; }
        public HMessage InClientLink { get; set; }
        public HMessage InfoFeedEnabled { get; set; }
        public HMessage InfoHotelClosed { get; set; }
        public HMessage InfoHotelClosing { get; set; }
        public HMessage InitCamera { get; set; }
        public HMessage InitDiffieHandshake { get; set; }
        public HMessage Interstitial { get; set; }
        public HMessage InventoryAddEffect { get; set; }
        public HMessage InventoryBadges { get; set; }
        public HMessage InventoryBots { get; set; }
        public HMessage InventoryItems { get; set; }
        public HMessage InventoryPets { get; set; }
        public HMessage InventoryRefresh { get; set; }
        public HMessage IsFirstLoginOfDay { get; set; }
        public HMessage IsOfferGiftable { get; set; }
        public HMessage IssueCloseNotification { get; set; }
        public HMessage ItemAdd { get; set; }
        public HMessage ItemDataUpdate { get; set; }
        public HMessage ItemRemove { get; set; }
        public HMessage ItemState { get; set; }
        public HMessage ItemUpdate { get; set; }
        public HMessage Items { get; set; }
        public HMessage ItemsDataUpdate { get; set; }
        public HMessage JukeBoxMySongs { get; set; }
        public HMessage JukeBoxNowPlayingMessage { get; set; }
        public HMessage JukeBoxPlayList { get; set; }
        public HMessage JukeBoxPlayListAddSong { get; set; }
        public HMessage JukeBoxPlayListUpdated { get; set; }
        public HMessage JukeBoxPlaylistFull { get; set; }
        public HMessage JukeBoxTrackCode { get; set; }
        public HMessage JukeBoxTrackData { get; set; }
        public HMessage LatencyPingResponse { get; set; }
        public HMessage LeprechaunStarterBundle { get; set; }
        public HMessage LimitedEditionSoldOut { get; set; }
        public HMessage LimitedOfferAppearingNext { get; set; }
        public HMessage LoginFailedHotelClosed { get; set; }
        public HMessage LoveLockFurniFinished { get; set; }
        public HMessage LoveLockFurniFriendConfirmed { get; set; }
        public HMessage LoveLockFurniStart { get; set; }
        public HMessage MOTDNotification { get; set; }
        public HMessage MaintenanceStatus { get; set; }
        public HMessage MarketplaceBuyOfferResult { get; set; }
        public HMessage MarketplaceCancelSale { get; set; }
        public HMessage MarketplaceConfiguration { get; set; }
        public HMessage MarketplaceItemStats { get; set; }
        public HMessage MarketplaceMakeOfferResult { get; set; }
        public HMessage MarketplaceOffers { get; set; }
        public HMessage MarketplaceOwnOffers { get; set; }
        public HMessage MarketplaceSellItem { get; set; }
        public HMessage MeMenuSettings { get; set; }
        public HMessage MessengerError { get; set; }
        public HMessage MessengerInit { get; set; }
        public HMessage MinimailCount { get; set; }
        public HMessage MinimailNewMessage { get; set; }
        public HMessage ModTool { get; set; }
        public HMessage ModToolComposerOne { get; set; }
        public HMessage ModToolComposerTwo { get; set; }
        public HMessage ModToolIssueChatlog { get; set; }
        public HMessage ModToolIssueHandlerDimensions { get; set; }
        public HMessage ModToolIssueInfo { get; set; }
        public HMessage ModToolIssueResponseAlert { get; set; }
        public HMessage ModToolIssueUpdate { get; set; }
        public HMessage ModToolReportReceivedAlert { get; set; }
        public HMessage ModToolRoomChatlog { get; set; }
        public HMessage ModToolRoomInfo { get; set; }
        public HMessage ModToolSanctionData { get; set; }
        public HMessage ModToolUserChatlog { get; set; }
        public HMessage ModToolUserInfo { get; set; }
        public HMessage ModToolUserRoomVisits { get; set; }
        public HMessage ModeratorCaution { get; set; }
        public HMessage ModeratorMessage { get; set; }
        public HMessage MoodLightData { get; set; }
        public HMessage MutedWhisper { get; set; }
        public HMessage MysteryBoxKeys { get; set; }
        public HMessage NavigatorEventCategories { get; set; }
        public HMessage NavigatorLiftedRooms { get; set; }
        public HMessage NavigatorMetaData { get; set; }
        public HMessage NavigatorSavedSearches { get; set; }
        public HMessage NavigatorSearchResults { get; set; }
        public HMessage NavigatorSettings { get; set; }
        public HMessage NewNavigatorCategoryUserCount { get; set; }
        public HMessage NewNavigatorSettings { get; set; }
        public HMessage NewUserGift { get; set; }
        public HMessage NewYearResolution { get; set; }
        public HMessage NewYearResolutionCompleted { get; set; }
        public HMessage NewYearResolutionProgress { get; set; }
        public HMessage NewsWidgets { get; set; }
        public HMessage NoobnessLevel { get; set; }
        public HMessage NotEnoughBalance { get; set; }
        public HMessage NotificationDialog { get; set; }
        public HMessage ObjectAdd { get; set; }
        public HMessage ObjectDataUpdate { get; set; }
        public HMessage ObjectRemove { get; set; }
        public HMessage Objects { get; set; }
        public HMessage OfferRewardDelivered { get; set; }
        public HMessage OldPublicRooms { get; set; }
        public HMessage OpenConnection { get; set; }
        public HMessage OpenRoomCreationWindow { get; set; }
        public HMessage OtherTradingDisabled { get; set; }
        public HMessage PeerUsersClassification { get; set; }
        public HMessage PerkAllowances { get; set; }
        public HMessage PetBoughtNotification { get; set; }
        public HMessage PetBreedingCompleted { get; set; }
        public HMessage PetBreedingFailed { get; set; }
        public HMessage PetBreedingResult { get; set; }
        public HMessage PetBreedingStart { get; set; }
        public HMessage PetBreedingStartFailed { get; set; }
        public HMessage PetError { get; set; }
        public HMessage PetInfo { get; set; }
        public HMessage PetLevelUp { get; set; }
        public HMessage PetLevelUpdated { get; set; }
        public HMessage PetNameError { get; set; }
        public HMessage PetPackageNameValidation { get; set; }
        public HMessage PetStatusUpdate { get; set; }
        public HMessage PetTrainingPanel { get; set; }
        public HMessage Ping { get; set; }
        public HMessage PollQuestions { get; set; }
        public HMessage PollStart { get; set; }
        public HMessage PostItStickyPoleOpen { get; set; }
        public HMessage PresentItemOpened { get; set; }
        public HMessage PrivateRooms { get; set; }
        public HMessage ProductOffer { get; set; }
        public HMessage PublicRooms { get; set; }
        public HMessage PurchaseError { get; set; }
        public HMessage PurchaseNotAllowed { get; set; }
        public HMessage PurchaseOK { get; set; }
        public HMessage QuestCompleted { get; set; }
        public HMessage QuestExpired { get; set; }
        public HMessage QuizData { get; set; }
        public HMessage QuizResults { get; set; }
        public HMessage RecyclerComplete { get; set; }
        public HMessage RecyclerLogic { get; set; }
        public HMessage RelationshipStatusInfo { get; set; }
        public HMessage ReloadRecycler { get; set; }
        public HMessage RemoveBot { get; set; }
        public HMessage RemoveFriend { get; set; }
        public HMessage RemoveGuildFromRoom { get; set; }
        public HMessage RemovePet { get; set; }
        public HMessage RentableItemBuyOutPrice { get; set; }
        public HMessage RentableSpaceInfo { get; set; }
        public HMessage ReportRoomForm { get; set; }
        public HMessage RoomAccessDenied { get; set; }
        public HMessage RoomAdError { get; set; }
        public HMessage RoomAdPurchaseInfo { get; set; }
        public HMessage RoomAddRightsList { get; set; }
        public HMessage RoomBannedUsers { get; set; }
        public HMessage RoomCategories { get; set; }
        public HMessage RoomCategoryUpdateMessage { get; set; }
        public HMessage RoomChatSettings { get; set; }
        public HMessage RoomEditSettingsError { get; set; }
        public HMessage RoomEnterError { get; set; }
        public HMessage RoomEntryInfo { get; set; }
        public HMessage RoomEvent { get; set; }
        public HMessage RoomEventCancel { get; set; }
        public HMessage RoomFilterWords { get; set; }
        public HMessage RoomFloorThicknessUpdated { get; set; }
        public HMessage RoomInvite { get; set; }
        public HMessage RoomInviteError { get; set; }
        public HMessage RoomMessagesPostedCount { get; set; }
        public HMessage RoomMuted { get; set; }
        public HMessage RoomOwner { get; set; }
        public HMessage RoomPaint { get; set; }
        public HMessage RoomPetExperience { get; set; }
        public HMessage RoomPetHorseFigure { get; set; }
        public HMessage RoomPetRespect { get; set; }
        public HMessage RoomQueueStatus { get; set; }
        public HMessage RoomRating { get; set; }
        public HMessage RoomReady { get; set; }
        public HMessage RoomRemoveRightsList { get; set; }
        public HMessage RoomRightsList { get; set; }
        public HMessage RoomSettings { get; set; }
        public HMessage RoomSettingsSaved { get; set; }
        public HMessage RoomSettingsUpdated { get; set; }
        public HMessage RoomUserChange { get; set; }
        public HMessage RoomUserData { get; set; }
        public HMessage RoomUserIgnored { get; set; }
        public HMessage RoomUserNameChanged { get; set; }
        public HMessage RoomUserReceivedHandItem { get; set; }
        public HMessage RoomUserRemoveRights { get; set; }
        public HMessage RoomUserRespect { get; set; }
        public HMessage RoomUserTags { get; set; }
        public HMessage RoomUserUnbanned { get; set; }
        public HMessage RoomVisualizationSettings { get; set; }
        public HMessage SanctionStatus { get; set; }
        public HMessage SeasonalCalendarDailyOffer { get; set; }
        public HMessage SellablePetPalettes { get; set; }
        public HMessage Shout { get; set; }
        public HMessage ShowMysteryBoxWait { get; set; }
        public HMessage SimplePollAnswer { get; set; }
        public HMessage SimplePollAnswers { get; set; }
        public HMessage SimplePollStart { get; set; }
        public HMessage Sleep { get; set; }
        public HMessage SlideObjectBundle { get; set; }
        public HMessage SubmitCompetitionRoom { get; set; }
        public HMessage Tags { get; set; }
        public HMessage TalentLevelUpdate { get; set; }
        public HMessage TalentTrack { get; set; }
        public HMessage TalentTrackEmailFailed { get; set; }
        public HMessage TalentTrackEmailVerified { get; set; }
        public HMessage TargetedOffer { get; set; }
        public HMessage TargetedOfferNotFound { get; set; }
        public HMessage TradeAccepted { get; set; }
        public HMessage TradeCloseWindow { get; set; }
        public HMessage TradeComplete { get; set; }
        public HMessage TradeStart { get; set; }
        public HMessage TradeStartFail { get; set; }
        public HMessage TradeStopped { get; set; }
        public HMessage TradeUpdate { get; set; }
        public HMessage TradingWaitingConfirm { get; set; }
        public HMessage UniqueMachineID { get; set; }
        public HMessage UnseenItems { get; set; }
        public HMessage UpdateFailed { get; set; }
        public HMessage UpdateStackHeightTileHeight { get; set; }
        public HMessage UpdateUserLook { get; set; }
        public HMessage UserBadges { get; set; }
        public HMessage UserBanned { get; set; }
        public HMessage UserCitizenship { get; set; }
        public HMessage UserClub { get; set; }
        public HMessage UserInfo { get; set; }
        public HMessage UserObject { get; set; }
        public HMessage UserPoints { get; set; }
        public HMessage UserProfile { get; set; }
        public HMessage UserRemove { get; set; }
        public HMessage UserRights { get; set; }
        public HMessage UserSearchResult { get; set; }
        public HMessage UserTyping { get; set; }
        public HMessage UserUpdate { get; set; }
        public HMessage UserWardrobe { get; set; }
        public HMessage Users { get; set; }
        public HMessage VerifyMobileNumber { get; set; }
        public HMessage VerifyMobilePhoneCodeWindow { get; set; }
        public HMessage VerifyMobilePhoneDone { get; set; }
        public HMessage VerifyMobilePhoneWindow { get; set; }
        public HMessage VipTutorialsStart { get; set; }
        public HMessage VoucherRedeemError { get; set; }
        public HMessage VoucherRedeemOk { get; set; }
        public HMessage WelcomeGift { get; set; }
        public HMessage WelcomeGiftError { get; set; }
        public HMessage Whisper { get; set; }
        public HMessage WiredConditionData { get; set; }
        public HMessage WiredEffectData { get; set; }
        public HMessage WiredRewardAlert { get; set; }
        public HMessage WiredSaved { get; set; }
        public HMessage WiredTriggerData { get; set; }
        public HMessage YouAreController { get; set; }
        public HMessage YouAreNotController { get; set; }
        public HMessage YouAreOwner { get; set; }
        public HMessage YouArePlayingGame { get; set; }
        public HMessage YouAreSpectator { get; set; }
        public HMessage YouTradingDisabled { get; set; }
        public HMessage YoutubeDisplayList { get; set; }

        public Incoming()
            : base(false)
        { }
        public Incoming(int capacity)
            : base(false, capacity)
        { }
        public Incoming(IList<HMessage> messages)
            : base(false, messages)
        { }
    }
}
