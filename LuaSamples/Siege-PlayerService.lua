-- Global Variables
local Knit = require(game:GetService("ReplicatedStorage").Packages.Knit)

local PlayerService = Knit.CreateService({
	Name = "PlayerService",
	Client = {}
})

local Players = game:GetService("Players")

function PlayerService.Client:MakeJump(player: Player)
	player.Character.Humanoid.Jump = true
end

function PlayerService.KnitInit()
	local Signal = require(Knit.Util.Signal)
	PlayerService.PlayerJoined = Signal.new()
	PlayerService.PlayerLeaving = Signal.new()
end

function PlayerService.KnitStart()
	-- Cover any future players
	Players.PlayerAdded:Connect(function(player: Player)
		PlayerService.PlayerJoined:Fire(player)
	end)
	
	-- Cover any players that joined during Knit initialization
	for _,player in pairs(Players:GetPlayers()) do
		PlayerService.PlayerJoined:Fire(player)
	end
	
	-- Players leaving
	Players.PlayerRemoving:Connect(function(player:Player)
		PlayerService.PlayerLeaving:Fire(player)
	end)
end


return PlayerService