﻿using System.Collections.Generic;

namespace Sulakore.Habbo.Messages
{
    public sealed class Outgoing : HMessages
    {
        public HMessage AcceptFriend { get; set; }
        public HMessage ActivateEffect { get; set; }
        public HMessage AddAdminRightsToMember { get; set; }
        public HMessage AdventCalendarForceOpen { get; set; }
        public HMessage AdventCalendarOpenDay { get; set; }
        public HMessage AdvertisingSave { get; set; }
        public HMessage AmbassadorAlertCommand { get; set; }
        public HMessage AmbassadorVisitCommand { get; set; }
        public HMessage ApproveAllMembershipRequests { get; set; }
        public HMessage ApproveMembershipRequest { get; set; }
        public HMessage ApproveName { get; set; }
        public HMessage AssignRights { get; set; }
        public HMessage AvatarExpression { get; set; }
        public HMessage BotSaveSettings { get; set; }
        public HMessage BotSettings { get; set; }
        public HMessage BreedPets { get; set; }
        public HMessage BuyMarketplaceOffer { get; set; }
        public HMessage BuyMarketplaceTokens { get; set; }
        public HMessage BuyRoomPromotion { get; set; }
        public HMessage CallForHelp { get; set; }
        public HMessage CancelMarketplaceOffer { get; set; }
        public HMessage CancelPoll { get; set; }
        public HMessage CancelTyping { get; set; }
        public HMessage CatalogBuyItem { get; set; }
        public HMessage CatalogBuyItemAsGift { get; set; }
        public HMessage CatalogSearchedItem { get; set; }
        public HMessage ChangeEmail { get; set; }
        public HMessage ChangeMotto { get; set; }
        public HMessage ChangePosture { get; set; }
        public HMessage ChangeQueue { get; set; }
        public HMessage ChangeRelation { get; set; }
        public HMessage Chat { get; set; }
        public HMessage ChatReviewGuideDecidesOnOffer { get; set; }
        public HMessage ChatReviewGuideVote { get; set; }
        public HMessage ChatReviewSessionCreate { get; set; }
        public HMessage CheckPetName { get; set; }
        public HMessage ClientHello { get; set; }
        public HMessage CloseIssues { get; set; }
        public HMessage CompleteDiffieHandshake { get; set; }
        public HMessage CompostPlant { get; set; }
        public HMessage ConfirmChangeName { get; set; }
        public HMessage ConvertGlobalRoomId { get; set; }
        public HMessage CraftSecret { get; set; }
        public HMessage CraftingAddRecipe { get; set; }
        public HMessage CraftingCraftItem { get; set; }
        public HMessage CreateFlat { get; set; }
        public HMessage CreateGuild { get; set; }
        public HMessage CustomizeAvatarWithFurni { get; set; }
        public HMessage Dance { get; set; }
        public HMessage DeactivateGuild { get; set; }
        public HMessage DeclineFriend { get; set; }
        public HMessage DeselectFavouriteHabboGroup { get; set; }
        public HMessage DiceOff { get; set; }
        public HMessage DropCarryItem { get; set; }
        public HMessage EditRoomPromotionMessage { get; set; }
        public HMessage EnableEffect { get; set; }
        public HMessage EventLog { get; set; }
        public HMessage FindNewFriends { get; set; }
        public HMessage FloorPlanEditorRequestBlockedTiles { get; set; }
        public HMessage FloorPlanEditorRequestDoorSettings { get; set; }
        public HMessage FloorPlanEditorSave { get; set; }
        public HMessage FollowFriend { get; set; }
        public HMessage FootballGateSaveLook { get; set; }
        public HMessage FriendListUpdate { get; set; }
        public HMessage FriendPrivateMessage { get; set; }
        public HMessage FriendRequest { get; set; }
        public HMessage Game2GetAccountGameStatus { get; set; }
        public HMessage Game2LoadStageReady { get; set; }
        public HMessage GameCenter { get; set; }
        public HMessage GetBadgePointLimits { get; set; }
        public HMessage GetBadges { get; set; }
        public HMessage GetBonusRareInfo { get; set; }
        public HMessage GetBundleDiscount { get; set; }
        public HMessage GetCatalogIndex { get; set; }
        public HMessage GetCfhStatus { get; set; }
        public HMessage GetClubData { get; set; }
        public HMessage GetClubGiftInfo { get; set; }
        public HMessage GetClubOffers { get; set; }
        public HMessage GetCraftingRecipesAvailable { get; set; }
        public HMessage GetCurrentTimingCode { get; set; }
        public HMessage GetExtendedProfile { get; set; }
        public HMessage GetExtendedProfileByName { get; set; }
        public HMessage GetFriendRequests { get; set; }
        public HMessage GetFurnitureAliases { get; set; }
        public HMessage GetGameStatus { get; set; }
        public HMessage GetGuestRoomData { get; set; }
        public HMessage GetGuildEditInfo { get; set; }
        public HMessage GetGuildEditorData { get; set; }
        public HMessage GetGuildMembers { get; set; }
        public HMessage GetHabboGroupDetails { get; set; }
        public HMessage GetHabboGuildBadges { get; set; }
        public HMessage GetIgnoredUsers { get; set; }
        public HMessage GetInterstitial { get; set; }
        public HMessage GetItemData { get; set; }
        public HMessage GetMarketplaceCanMakeOffer { get; set; }
        public HMessage GetMarketplaceConfiguration { get; set; }
        public HMessage GetMarketplaceItemStats { get; set; }
        public HMessage GetMarketplaceOffers { get; set; }
        public HMessage GetMarketplaceOwnOffers { get; set; }
        public HMessage GetPetCommands { get; set; }
        public HMessage GetPollData { get; set; }
        public HMessage GetQuizQuestions { get; set; }
        public HMessage GetRelationshipStatusInfo { get; set; }
        public HMessage GetRentOrBuyoutOffer { get; set; }
        public HMessage GetRoomEntryData { get; set; }
        public HMessage GetSelectedBadges { get; set; }
        public HMessage GetSellablePetPalettes { get; set; }
        public HMessage GetTalentTrack { get; set; }
        public HMessage GetTalentTrackLevel { get; set; }
        public HMessage GetTargetedOffer { get; set; }
        public HMessage GetUserProfileById { get; set; }
        public HMessage GetUserProfileByName { get; set; }
        public HMessage GiveSupplementToPet { get; set; }
        public HMessage GuardianNoUpdatesWanted { get; set; }
        public HMessage GuideAdvertisementRead { get; set; }
        public HMessage GuideSessionFeedback { get; set; }
        public HMessage GuideSessionGetRequesterRoom { get; set; }
        public HMessage GuideSessionGuideDecides { get; set; }
        public HMessage GuideSessionInviteRequester { get; set; }
        public HMessage GuideSessionIsTyping { get; set; }
        public HMessage GuideSessionMessage { get; set; }
        public HMessage GuideSessionReport { get; set; }
        public HMessage GuideSessionRequesterCancels { get; set; }
        public HMessage GuideSessionResolved { get; set; }
        public HMessage GuildConfirmRemoveMember { get; set; }
        public HMessage HorseRideSettings { get; set; }
        public HMessage HorseUseItem { get; set; }
        public HMessage HotelView { get; set; }
        public HMessage HotelViewClaimBadge { get; set; }
        public HMessage HotelViewClaimBadgeReward { get; set; }
        public HMessage HotelViewConcurrentUsersButton { get; set; }
        public HMessage HotelViewRequestBadgeReward { get; set; }
        public HMessage HotelViewRequestCommunityGoal { get; set; }
        public HMessage HotelViewRequestConcurrentUsers { get; set; }
        public HMessage IgnoreUser { get; set; }
        public HMessage IgnoreUserId { get; set; }
        public HMessage InfoRetrieve { get; set; }
        public HMessage InitCamera { get; set; }
        public HMessage InitDiffieHandshake { get; set; }
        public HMessage JoinHabboGroup { get; set; }
        public HMessage JoinQueue { get; set; }
        public HMessage JukeBoxAddSoundTrack { get; set; }
        public HMessage JukeBoxEventOne { get; set; }
        public HMessage JukeBoxEventThree { get; set; }
        public HMessage JukeBoxEventTwo { get; set; }
        public HMessage JukeBoxRequestPlayList { get; set; }
        public HMessage JukeBoxRequestTrackCode { get; set; }
        public HMessage JukeBoxRequestTrackData { get; set; }
        public HMessage KickMember { get; set; }
        public HMessage LagWarningReport { get; set; }
        public HMessage LatencyPingReport { get; set; }
        public HMessage LatencyPingRequest { get; set; }
        public HMessage LetUserIn { get; set; }
        public HMessage LookTo { get; set; }
        public HMessage MakeOffer { get; set; }
        public HMessage MannequinSaveLook { get; set; }
        public HMessage MannequinSaveName { get; set; }
        public HMessage ModToolAlert { get; set; }
        public HMessage ModToolBan { get; set; }
        public HMessage ModToolChangeRoomSettings { get; set; }
        public HMessage ModToolKick { get; set; }
        public HMessage ModToolRequestIssueChatlog { get; set; }
        public HMessage ModToolRequestRoomChatlog { get; set; }
        public HMessage ModToolRequestRoomInfo { get; set; }
        public HMessage ModToolRequestRoomUserChatlog { get; set; }
        public HMessage ModToolRequestRoomVisits { get; set; }
        public HMessage ModToolRequestUserChatlog { get; set; }
        public HMessage ModToolRequestUserInfo { get; set; }
        public HMessage ModToolRoomAlert { get; set; }
        public HMessage ModToolSanctionAlert { get; set; }
        public HMessage ModToolSanctionBan { get; set; }
        public HMessage ModToolSanctionMute { get; set; }
        public HMessage ModToolSanctionTradeLock { get; set; }
        public HMessage ModToolWarn { get; set; }
        public HMessage MoodLightSaveSettings { get; set; }
        public HMessage MoodLightSettings { get; set; }
        public HMessage MoodLightTurnOn { get; set; }
        public HMessage MountPet { get; set; }
        public HMessage MoveAvatar { get; set; }
        public HMessage MoveObject { get; set; }
        public HMessage MovePet { get; set; }
        public HMessage MoveWallItem { get; set; }
        public HMessage NavigatorAddSavedSearch { get; set; }
        public HMessage NavigatorCategoryListMode { get; set; }
        public HMessage NavigatorCollapseCategory { get; set; }
        public HMessage NavigatorDeleteSavedSearch { get; set; }
        public HMessage NavigatorUncollapseCategory { get; set; }
        public HMessage NewNavigatorAction { get; set; }
        public HMessage NuxGetGifts { get; set; }
        public HMessage NuxScriptProceed { get; set; }
        public HMessage OpenFlatConnection { get; set; }
        public HMessage OpenRecycleBox { get; set; }
        public HMessage PassCarryItem { get; set; }
        public HMessage PassCarryItemToPet { get; set; }
        public HMessage PeerUsersClassification { get; set; }
        public HMessage PerformanceLog { get; set; }
        public HMessage PetPackageName { get; set; }
        public HMessage PetSelected { get; set; }
        public HMessage PickIssues { get; set; }
        public HMessage PickNewUserGift { get; set; }
        public HMessage PickupObject { get; set; }
        public HMessage PlaceBot { get; set; }
        public HMessage PlaceObject { get; set; }
        public HMessage PlacePet { get; set; }
        public HMessage PollAnswer { get; set; }
        public HMessage Pong { get; set; }
        public HMessage PostItPlace { get; set; }
        public HMessage PostQuizAnswers { get; set; }
        public HMessage PublishPhoto { get; set; }
        public HMessage PurchasePhoto { get; set; }
        public HMessage RateFlat { get; set; }
        public HMessage Recycle { get; set; }
        public HMessage RedeemClothing { get; set; }
        public HMessage RedeemItem { get; set; }
        public HMessage RedeemVoucher { get; set; }
        public HMessage RejectMembershipRequest { get; set; }
        public HMessage ReleaseIssues { get; set; }
        public HMessage ReloadRecycler { get; set; }
        public HMessage RemoveAdminRightsFromMember { get; set; }
        public HMessage RemoveBotFromFlat { get; set; }
        public HMessage RemoveFriend { get; set; }
        public HMessage RemoveItem { get; set; }
        public HMessage RemovePetFromFlat { get; set; }
        public HMessage RemoveRights { get; set; }
        public HMessage RemoveSaddleFromPet { get; set; }
        public HMessage RenderRoom { get; set; }
        public HMessage RenderRoomThumbnail { get; set; }
        public HMessage RentSpace { get; set; }
        public HMessage RentSpaceCancel { get; set; }
        public HMessage RequestAchievements { get; set; }
        public HMessage RequestCanCreateRoom { get; set; }
        public HMessage RequestCatalogIndex { get; set; }
        public HMessage RequestCatalogMode { get; set; }
        public HMessage RequestCatalogPage { get; set; }
        public HMessage RequestCraftingRecipes { get; set; }
        public HMessage RequestCredits { get; set; }
        public HMessage RequestDeleteRoom { get; set; }
        public HMessage RequestDiscount { get; set; }
        public HMessage RequestFriends { get; set; }
        public HMessage RequestGameConfigurations { get; set; }
        public HMessage RequestGiftConfiguration { get; set; }
        public HMessage RequestGuideAssistance { get; set; }
        public HMessage RequestGuideTool { get; set; }
        public HMessage RequestGuildBuyRooms { get; set; }
        public HMessage RequestGuildFurniWidget { get; set; }
        public HMessage RequestGuildManage { get; set; }
        public HMessage RequestHighestScoreRooms { get; set; }
        public HMessage RequestInitFriends { get; set; }
        public HMessage RequestInventoryBots { get; set; }
        public HMessage RequestInventoryItems { get; set; }
        public HMessage RequestInventoryPets { get; set; }
        public HMessage RequestMeMenuSettings { get; set; }
        public HMessage RequestMyRooms { get; set; }
        public HMessage RequestNavigatorSettings { get; set; }
        public HMessage RequestNewNavigatorData { get; set; }
        public HMessage RequestNewNavigatorRooms { get; set; }
        public HMessage RequestNewsList { get; set; }
        public HMessage RequestOwnGuilds { get; set; }
        public HMessage RequestPetBreeds { get; set; }
        public HMessage RequestPetInfo { get; set; }
        public HMessage RequestPopularRooms { get; set; }
        public HMessage RequestPromotedRooms { get; set; }
        public HMessage RequestPromotionRooms { get; set; }
        public HMessage RequestPublicRooms { get; set; }
        public HMessage RequestRecylerLogic { get; set; }
        public HMessage RequestReportRoom { get; set; }
        public HMessage RequestReportUserBullying { get; set; }
        public HMessage RequestResolution { get; set; }
        public HMessage RequestRoomCategories { get; set; }
        public HMessage RequestRoomRights { get; set; }
        public HMessage RequestRoomSettings { get; set; }
        public HMessage RequestRoomWordFilter { get; set; }
        public HMessage RequestTags { get; set; }
        public HMessage RequestUserClub { get; set; }
        public HMessage RequestUserCredits { get; set; }
        public HMessage RequestUserTags { get; set; }
        public HMessage RequestUserWardrobe { get; set; }
        public HMessage ResetUnseenItemIds { get; set; }
        public HMessage ResetUnseenItems { get; set; }
        public HMessage RespectUser { get; set; }
        public HMessage RoomAdPurchaseInitiated { get; set; }
        public HMessage RoomBackground { get; set; }
        public HMessage RoomCompetitionInit { get; set; }
        public HMessage RoomFavorite { get; set; }
        public HMessage RoomMute { get; set; }
        public HMessage RoomNetworkOpenConnection { get; set; }
        public HMessage RoomPlaceBuildersClubItem { get; set; }
        public HMessage RoomRemoveAllRights { get; set; }
        public HMessage RoomRemoveRights { get; set; }
        public HMessage RoomRequestBannedUsers { get; set; }
        public HMessage RoomSettingsSave { get; set; }
        public HMessage RoomStaffPick { get; set; }
        public HMessage RoomUserBan { get; set; }
        public HMessage RoomUserKick { get; set; }
        public HMessage RoomUserMute { get; set; }
        public HMessage RoomUsersClassification { get; set; }
        public HMessage RoomWordFilterModify { get; set; }
        public HMessage SSOTicket { get; set; }
        public HMessage SaveBlockCameraFollow { get; set; }
        public HMessage SaveIgnoreRoomInvites { get; set; }
        public HMessage SavePostItStickyPole { get; set; }
        public HMessage SavePreferOldChat { get; set; }
        public HMessage SaveUserVolumes { get; set; }
        public HMessage SaveWardrobe { get; set; }
        public HMessage SaveWindowSettings { get; set; }
        public HMessage ScratchPet { get; set; }
        public HMessage SearchRooms { get; set; }
        public HMessage SearchRoomsByTag { get; set; }
        public HMessage SearchRoomsFriendsNow { get; set; }
        public HMessage SearchRoomsFriendsOwn { get; set; }
        public HMessage SearchRoomsInGroup { get; set; }
        public HMessage SearchRoomsMyFavorite { get; set; }
        public HMessage SearchRoomsVisited { get; set; }
        public HMessage SearchRoomsWithRights { get; set; }
        public HMessage SearchUser { get; set; }
        public HMessage SelectClubGift { get; set; }
        public HMessage SelectFavouriteHabboGroup { get; set; }
        public HMessage SendRoomInvite { get; set; }
        public HMessage SetActivatedBadges { get; set; }
        public HMessage SetClothingChangeData { get; set; }
        public HMessage SetHomeRoom { get; set; }
        public HMessage SetItemData { get; set; }
        public HMessage SetObjectData { get; set; }
        public HMessage SetStackHelperHeight { get; set; }
        public HMessage Shout { get; set; }
        public HMessage Sign { get; set; }
        public HMessage StartTyping { get; set; }
        public HMessage TestInventory { get; set; }
        public HMessage ThrowDice { get; set; }
        public HMessage TogglePetBreedingPermission { get; set; }
        public HMessage TogglePetRidingPermission { get; set; }
        public HMessage TradeAccept { get; set; }
        public HMessage TradeCancelOfferItem { get; set; }
        public HMessage TradeClose { get; set; }
        public HMessage TradeConfirm { get; set; }
        public HMessage TradeOfferItem { get; set; }
        public HMessage TradeOfferMultipleItems { get; set; }
        public HMessage TradeStart { get; set; }
        public HMessage TradeUnAccept { get; set; }
        public HMessage TriggerColorWheel { get; set; }
        public HMessage TriggerOneWayGate { get; set; }
        public HMessage UnbanRoomUser { get; set; }
        public HMessage UnignoreUser { get; set; }
        public HMessage UniqueID { get; set; }
        public HMessage UpdateGuildBadge { get; set; }
        public HMessage UpdateGuildColors { get; set; }
        public HMessage UpdateGuildIdentity { get; set; }
        public HMessage UpdateGuildSettings { get; set; }
        public HMessage UseFurniture { get; set; }
        public HMessage UseWallItem { get; set; }
        public HMessage UserSaveLook { get; set; }
        public HMessage Username { get; set; }
        public HMessage VersionCheck { get; set; }
        public HMessage Whisper { get; set; }
        public HMessage WiredConditionSaveData { get; set; }
        public HMessage WiredEffectSaveData { get; set; }
        public HMessage WiredTriggerSaveData { get; set; }
        public HMessage YoutubeRequestNextVideo { get; set; }
        public HMessage YoutubeRequestPlayList { get; set; }
        public HMessage YoutubeRequestVideoData { get; set; }

        public Outgoing()
            : base(true)
        { }
        public Outgoing(int capacity)
            : base(true, capacity)
        { }
        public Outgoing(IList<HMessage> messages)
            : base(true, messages)
        { }
    }
}
