--strict

local Minesweeper = {}

-- > DEPENDENCIES

---- Services

local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local TweenService = game:GetService("TweenService")

---- Modules

local Modules = ReplicatedStorage:WaitForChild("Modules")
local Network = require(Modules:WaitForChild("Network"))

---- Objects

local Player = Players.LocalPlayer
local PlayerGui = Player.PlayerGui

local gui: ScreenGui = PlayerGui:WaitForChild("MinesweeperGui")

local mainFrame: Frame = gui:WaitForChild("MainFrame")
local gameContent: Frame = mainFrame:WaitForChild("GameContent")

local startFrame: Frame = gameContent:WaitForChild("Start")
local gameFrame: Frame = gameContent:WaitForChild("Game")
local endFrame: Frame = gameContent:WaitForChild("End")

local contentFrames = {startFrame, gameFrame, endFrame}

local template = script:WaitForChild("Template")

---- Constants

local USE_BG_COLOUR = true

local PANEL_TWEENINFO = TweenInfo.new(
	0.75,
	Enum.EasingStyle.Back,
	Enum.EasingDirection.In
)

local PANEL_POSITION_OPEN = UDim2.fromScale(0.5, 0.5)
local PANEL_POSITION_CLOSED = UDim2.fromScale(0.5, -0.5)

local DEBOUNCE_TIME = 0.1

local SWIPE_TWEENINFO = TweenInfo.new(
	1,
	Enum.EasingStyle.Cubic,
	Enum.EasingDirection.In
)

local TILE_COLORS = {
	unknown = Color3.fromRGB(55, 57, 62),
	known = Color3.fromRGB(121, 125, 136),
	flagged = Color3.fromHex("93000a"),
	flaggedText = Color3.fromHex("ffdad6")
}

local NUMBER_TILE_COLORS = {
	-- 1
	{
		Color3.fromHex("004c6a"),
		Color3.fromHex("c5e7ff")
	},
	-- 2
	{
		Color3.fromHex("294f20"),
		Color3.fromHex("c1efaf")
	},
	-- 3
	{
		Color3.fromHex("73342c"),
		Color3.fromHex("ffdad5")
	},
	-- 4
	{
		Color3.fromHex("703349"),
		Color3.fromHex("ffd9e2")
	},
}

local BOARD_SIZE = 10

---- Global Variables

Minesweeper.mines = {} -- States: true = mine, false = no mine
Minesweeper.numbers = {} -- States: number = number of mines around, true = mine flagged, false = not dug/flagged yet

---- Local Variables

local panelOpen = false
local panelMoving = false

local swipePosition = 0
local swipeMoving = false

-- > GLOBAL FUNCTIONS

function Minesweeper.Toggle()
	if (panelOpen) then
		Minesweeper.Close()
	else
		Minesweeper.Open()
	end
end

--- Animates the game panel to be visible
function Minesweeper.Open()
	-- Debounce
	if panelMoving then return false end

	-- Data
	panelMoving = true

	-- Animate
	local tween = TweenService:Create(mainFrame, PANEL_TWEENINFO, {Position = PANEL_POSITION_OPEN})

	tween:Play()

	-- Data
	tween.Completed:Connect(function()
		panelOpen = true
		task.wait(DEBOUNCE_TIME)
		panelMoving = false
	end)

	return true
end


--- Animates the game panel to not be visible
function Minesweeper.Close()
	-- Debounce
	if panelMoving then return false end

	-- Data
	panelMoving = true

	-- Animate
	local tween = TweenService:Create(mainFrame, PANEL_TWEENINFO, {Position = PANEL_POSITION_CLOSED})

	tween:Play()

	-- Data
	tween.Completed:Connect(function()
		panelOpen = false
		task.wait(DEBOUNCE_TIME)
		panelMoving = false
	end)

	return true
end


--- Toggles the appearance/background colour styling of tiles on the board
function Minesweeper.ToggleBG()
	USE_BG_COLOUR = not USE_BG_COLOUR

	for x = 1, BOARD_SIZE do
		for y = 1, BOARD_SIZE do
			local num = Minesweeper.numbers[y][x]

			if (typeof(num) == "number" and num > 0) then
				updateTile(x, y, num)
			end
		end
	end
end


--- Swipes through the 3 content panels horizontally
function Minesweeper.SwipeContent(n: number)
	swipeContent(n)
end


--- Begins a new round and prepares everything
function Minesweeper.Begin()
	initialize()
end



-- > LOCAL FUNCTIONS



---- Initialization Functions

--- Returns a new map of randomly generated mines
function getNewMap()
	local mines = {}

	for y = 1, BOARD_SIZE do
		mines[y] = {}
		for x = 1, BOARD_SIZE do
			mines[y][x] = math.random() < 0.2
		end
	end

	return mines
end

function begin()

end

--- Moves the Start Game and End frames into their positions for the sliding animations
function initializeContent()
	mainFrame.Position = PANEL_POSITION_CLOSED

	for i = 1, 3 do
		print("i=" .. tostring(i))
		contentFrames[i].Position = UDim2.fromScale(i - 1, 0)
	end

	for _,child in pairs(gameFrame:GetChildren()) do
		if child:IsA("TextButton") then
			child:Destroy()
		end
	end

	Minesweeper.numbers = {}
	for y = 1, BOARD_SIZE do
		Minesweeper.numbers[y] = {}
		for x = 1, BOARD_SIZE do
			Minesweeper.numbers[y][x] = false
			local index = (y-1)*BOARD_SIZE+x

			local tile = template:Clone()
			tile.Name = index
			tile.LayoutOrder = index
			tile.Parent = gameFrame

			tile.MouseButton1Click:Connect(function()
				dig(x, y)
			end)

			tile.MouseButton2Click:Connect(function()
				toggleFlag(x, y)
			end)
		end
	end
end

--- Loads a new map and loads/moves UI components to their starting positions
function initialize()
	Minesweeper.mines = getNewMap()

	initializeContent()
end


---- Visual Functions

--- Animates the Start Game and End frames horizontally when switching game states
function swipeContent(page: number): boolean
	-- Debounce
	if swipeMoving then return false end
	-- No need to animate
	if swipePosition == page then return false end

	swipeMoving = true

	local tweenInfo = TweenInfo.new(
		1, -- math.abs(swipePosition - page) * 
		Enum.EasingStyle.Circular,
		Enum.EasingDirection.In
	)

	for i = 1, 3 do
		local tween = TweenService:Create(contentFrames[i], tweenInfo, {Position = UDim2.fromScale(i - 1 - (page - 1), 0)})
		tween:Play()

		if (i == 3) then
			tween.Completed:Connect(function()
				swipePosition = page
				task.wait(DEBOUNCE_TIME)
				swipeMoving = false
			end)
		end
	end

	-- TODO

	return true
end

--- Updates the state of a tile on the game screen
function updateTile(x: number, y: number, state: any)
	local tileNumber = (y-1)*BOARD_SIZE+x
	local tile = gameFrame[tileNumber]

	if (state == 0)	then -- tile has no mine and nothing around it, just empty tile with colour change
		tile.BackgroundColor3 = TILE_COLORS.known
		tile.Text = ""
	elseif (state == true) then -- tile has been flagged as a mine
		tile.BackgroundColor3 = TILE_COLORS.flagged
		tile.TextColor3 = TILE_COLORS.flaggedText
		tile.Text = "*"
	elseif (state == false) then -- tile has been flagged as a mine
		tile.BackgroundColor3 = TILE_COLORS.unknown
		tile.Text = ""
	elseif (state <= 8) then -- tile has no mine but mines around it, show number & respective colour
		state = math.min(state, 4)
		
		if USE_BG_COLOUR then
			tile.BackgroundColor3 = NUMBER_TILE_COLORS[state][1]
			tile.TextColor3 = NUMBER_TILE_COLORS[state][2]
		else
			tile.BackgroundColor3 = TILE_COLORS.known
			tile.TextColor3 = NUMBER_TILE_COLORS[state][1]
		end

		tile.Text = state
	end
end


---- Utility functions

--- Attempts to dig at a specified location, if it has already been dug it reveals nearby safe tiles
function dig(x: number, y: number)
	-- Dig a tile that doesn't contain a mine
	if (Minesweeper.mines[y][x] == false) then
		
		-- This tile is currently unknown, dig and display a number
		if (Minesweeper.numbers[y][x] == false) then
			local mines = mineCountXY(x, y)

			Minesweeper.numbers[y][x] = mines
			updateTile(x, y, mines)

			if (mines == 0) then
				uncover(x, y)
			end
		-- This tile is known with a number shown
		else
			local flaggeds = flaggedCountXY(x, y)
			
			-- If this number has been satisfied with the number of flags around it, dig the rest that aren't flagged
			if (Minesweeper.numbers[y][x] == flaggeds) then
				uncover(x, y)
			end
		end
	-- Dig a mine (if it hasn't been flagged)
	elseif (Minesweeper.numbers[y][x] ~= true) then
		-- TODO: hit a mine, die
	end
end

--- Flags a given tile as a mine
function toggleFlag(x: number, y: number)
	-- If already flagged, unflag. If not flagged then flag it. Does not allow revealed number tiles to be flagged or unflagged.
	if (Minesweeper.numbers[y][x] == true) then
		Minesweeper.numbers[y][x] = false
		updateTile(x, y, false)
	elseif (Minesweeper.numbers[y][x] == false) then
		Minesweeper.numbers[y][x] = true
		updateTile(x, y, true)
	end
end

--- Attempts to uncover surrounding territory from a 0 tile or double clicked number with it's requirements satisfied
function uncover(x: number, y: number)
	local leftLimit = math.max(1, x-1)
	local rightLimit = math.min(BOARD_SIZE, x+1)
	local topLimit = math.max(1, y-1)
	local bottomLimit = math.min(BOARD_SIZE, y+1)

	for xx = leftLimit, rightLimit do
		for yy = topLimit, bottomLimit do
			if (xx == x and yy == y) then continue end

			if (Minesweeper.numbers[yy][xx] == false) then
				dig(xx, yy)
				--local mines = 
				--if (mines == 0) then
				--	uncover(xx, yy)
				--end
			end
		end
	end

	if (Minesweeper.numbers[y][x] == false) then
		local surround = surroundingMines(x, y)
		local mineCount = mineCount(surround)
		if (mineCount == 0) then
			uncover()
			--for xx = 1, BOARD_SIZE do
			--	for yy = 1, BOARD_SIZE do
			--		if (Minesweeper.numbers[yy][xx] == false) then
			--			dig(xx, yy)
			--		end
			--	end
			--end
		end
	end
end

--- Returns an array of the contents of all surrounding tiles (mine or not)
function surroundingMines(x: number, y: number)
	local result = {}

	print("x:", x)
	print("y:", y)

	local leftLimit = math.max(1, x-1)
	local rightLimit = math.min(BOARD_SIZE, x+1)
	local topLimit = math.max(1, y-1)
	local bottomLimit = math.min(BOARD_SIZE, y+1)

	for xx = leftLimit, rightLimit do
		for yy = topLimit, bottomLimit do
			if (xx == x and yy == y) then continue end
			table.insert(result, Minesweeper.mines[yy][xx])
		end
	end

	return result
end

--- Returns the number of mines around a given tile
function mineCount(surroundings: {})
	local count = 0

	for _, mine in pairs(surroundings) do
		if mine then count += 1 end
	end

	return count
end

--- Returns the number of mines around a given tile
function mineCountXY(x: number, y: number)
	return mineCount(surroundingMines(x, y))
end

--- Returns an array of the contents of all surrounding tiles (mine or not)
function surroundingNumbers(x: number, y: number)
	local result = {}

	print("x:", x)
	print("y:", y)

	local leftLimit = math.max(1, x-1)
	local rightLimit = math.min(BOARD_SIZE, x+1)
	local topLimit = math.max(1, y-1)
	local bottomLimit = math.min(BOARD_SIZE, y+1)

	for xx = leftLimit, rightLimit do
		for yy = topLimit, bottomLimit do
			if (xx == x and yy == y) then continue end
			table.insert(result, Minesweeper.mines[yy][xx])
		end
	end

	return result
end

--- Returns the number of mines around a given tile
function flaggedCount(surroundings: {})
	local count = 0

	for _, mine in pairs(surroundings) do
		if mine then count += 1 end
	end

	return count
end

--- Returns the number of mines around a given tile
function flaggedCountXY(x: number, y: number)
	return mineCount(surroundingNumbers(x, y))
end



-- > Initializer

initialize()


-- > Return

return Minesweeper