-- > ABOUT
-- Minesweeper testing
-- by keegaroo65 (roblox@133643347) (discord@kee65)

--> DEPENDENCIES

---- Services

local CollectionService = game:GetService("CollectionService")
local ContextActionService
local Players = game:GetService("Players")
local ReplicatedService = game:GetService("ReplicatedStorage")
local TweenService = game:GetService("TweenService")
local UserInputService = game:GetService("UserInputService")

---- Replicated Modules

local Modules = ReplicatedService.Modules
local Nature2D = require(Modules:WaitForChild("Nature2D"))
local Network = require(Modules:WaitForChild("Network"))

---- Client Modules

local MinesweeperGame = require(script:WaitForChild("MinesweeperGame"))

---- Objects

local Player = Players.LocalPlayer
local PlayerGui = Player.PlayerGui

---- User Interface

-- > INITIALIZATION

UserInputService.InputBegan:Connect(function(input, gameProcessed)
	if gameProcessed then return end

	if input.UserInputType == Enum.UserInputType.Keyboard then
		if input.KeyCode == Enum.KeyCode.T then
			MinesweeperGame.Toggle()
		elseif input.KeyCode == Enum.KeyCode.R then
			MinesweeperGame.Begin()
		elseif input.KeyCode == Enum.KeyCode.B then
			MinesweeperGame.ToggleBG()
		elseif input.KeyCode == Enum.KeyCode.One then
			MinesweeperGame.SwipeContent(1)
		elseif input.KeyCode == Enum.KeyCode.Two then
			MinesweeperGame.SwipeContent(2)
		elseif input.KeyCode == Enum.KeyCode.Three then
			MinesweeperGame.SwipeContent(3)
		end
	end
end)