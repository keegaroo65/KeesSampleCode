package ca.kee65.rummikub.data

class GameState {
    val boardState = BoardState()
    val playerState = PlayerState()
}

class BoardState {
    val runs = mutableListOf<TileRun>()
    val sets = mutableListOf<TileSet>()

    val isEmpty: Boolean
        get() = runs.isEmpty() && sets.isEmpty()

    init {
        runs.add(TileRun(TileColor.Blue, 1, 10))
        runs.add(TileRun(TileColor.Black, 7, 13))
        sets.add(TileSet(11, mutableListOf(TileColor.Red, TileColor.Black, TileColor.Blue, TileColor.Yellow)))
        sets.add(TileSet(3, mutableListOf(TileColor.Red, TileColor.Yellow, TileColor.Blue)))
        sets.add(TileSet(4, mutableListOf(TileColor.Yellow, TileColor.Red, TileColor.Blue)))
    }
}

class PlayerState {
    val tiles = mutableListOf<Tile>()
}

class TileRun(
    var color: TileColor,
    var numbers: MutableList<Int> = mutableListOf<Int>()
) {
    val length: Int
        get() = numbers.size

    constructor(
        color: TileColor,
        start: Int,
        end: Int
    ): this(color) {
        val numbers = mutableListOf<Int>()

        for (i in start..end) {
            numbers.add(i)
        }

        this.color = color
        this.numbers = numbers
    }

    override fun toString(): String {
        return "${color.toString()} run of ${numbers.joinToString(", ")}"
    }

    fun canAddLeft(color: TileColor, number: Int): Boolean {
        return color == this.color && numbers[0] == number + 1
    }
    fun canAddLeft(tile: Tile): Boolean = canAddLeft(tile.color, tile.number)


    fun canAddRight(color: TileColor, number: Int): Boolean {
        return color == this.color && numbers[numbers.size - 1] == number - 1
    }
    fun canAddRight(tile: Tile): Boolean = canAddRight(tile.color, tile.number)

    fun canTakeSafelyAt(index: Int): Boolean {
        if (index < 0 || index > length - 1) return false

        if (index == 0 || index == length - 1) {
            return length >= 4 // Can take one from end if leaves 3 or more in the run
        }
        else {
            val leftLength = index
            val rightLength = length - leftLength - 1

            if (leftLength >= 3 && rightLength >= 3) {
                return true
            }
        }

        return false
    }
    fun canTakeSafely(number: Int): Boolean {
        val index = numbers.indexOf(number)

        return canTakeSafelyAt(index)
    }

    fun addLeft(color: TileColor, number: Int) {
        if (!canAddLeft(color, number)) return

        numbers.add(0, number)
    }
    fun addLeft(tile: Tile) = addLeft(tile.color, tile.number)

    fun addRight(color: TileColor, number: Int) {
        if (!canAddRight(color, number)) return

        numbers.add(number)
    }
    fun addRight(tile: Tile) = addRight(tile.color, tile.number)

    // Index is the index of the first tile to be included on the RIGHT, everything before it goes on left.
    // Eg. (1,2,3,4,5).splitAt(2) = [(1,2), (3,4,5)]
    // Eg. (6,7,8,9,10,11).splitAt(3) = [(6,7,8), (9,10,11)]
    // Eg. (3,4,5,6).splitAt(3) = [(3,4,5), (6)]
    fun splitAt(index: Int): List<TileRun> {
        if (index < 0 || index > length - 1) return listOf()

        val left = numbers.subList(0, index)
        val right = numbers.subList(index, length)

        return listOf(
            TileRun(
                color,
                left
            ),
            TileRun(
                color,
                right
            )
        )
    }
    fun split(number: Int): List<TileRun> {
        val index = numbers.indexOf(number)

        return splitAt(index)
    }
}

class TileSet(
    val number: Int,
    val colors: MutableList<TileColor>
) {
    fun canAdd(color: TileColor): Boolean {
        return !colors.contains(color)
    }

    fun add(color: TileColor): Boolean {
        if (!canAdd(color)) return false

        colors.add(color)

        return true
    }

    fun canSafelyRemove(color: TileColor): Boolean {
        return colors.contains(color) && colors.count() > 3
    }

    fun remove(color: TileColor): Boolean {
        colors.remove(color)

        return true
    }
}