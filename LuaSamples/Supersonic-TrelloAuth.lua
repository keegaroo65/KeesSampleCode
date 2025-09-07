-- Global Variables

local BOARD_NAME = "Product Keys"

local DataManager = require(game.ServerStorage.DataManager)
local Trello = require(game.ServerStorage.TrelloAPI)

Keys = game.ReplicatedStorage.Networking.Keys

-- Local Variables

debounces = {}

boardId = Trello.GetBoardID(BOARD_NAME)

availableListId = Trello.GetListID(boardId, "Available")
usedListId = Trello.GetListID(boardId, "Used")

-- Global Functions

function CheckPlayer(player: Player)
	DataManager.YieldToLoaded(player)

	local authed = DataManager.Get(player,"Security.Authorized")

	--warn("authed",authed)

	return authed
end

function Attempt(player: Player, text: string)
	if CheckPlayer(player) then return true end -- Can't attempt if you are already whitelisted

	if not debounces[player] then
		debounces[player] = true

		local activeKeys = Trello.GetCardsOnList(availableListId)

		local authed = false

		for k,card in pairs(activeKeys) do
			if card.name == text then
				authed = true

				Trello.UpdateCard(card.id, {
					idList = usedListId,
					desc = card.desc.. string.format("\n\nUsed by: %s (%u)", player.Name, player.UserId)
				})
			--else
				--warn('card',card)
			end
		end

		if authed then
			--print("a",DataManager.Get(player,"Security.Authorized"))

			DataManager.Set(player,"Security.Authorized",true)

			--print("b",DataManager.Get(player,"Security.Authorized"))
		else
			--print("Not authed!")
		end

		task.delay(function()
			debounces[player] = nil
		end, 4.5)

		return authed
	end
end

function Process(id, ...)
	if id == "CheckPlayer" then
		return CheckPlayer(...)
	elseif id == "Attempt" then
		return Attempt(...)
	end
end

-- Initialization

Keys.OnServerInvoke = Attempt