package ca.kee65.busapp.data

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "trips")
data class Trip(
    @PrimaryKey(autoGenerate = true)
    val id: Int = 0,

    @ColumnInfo(name = "start_timestamp")
    val startTime: Long,

    @ColumnInfo(name = "end_timestamp")
    val endTime: Long = 0,

    @ColumnInfo(name = "route_id")
    val routeId: Int = 0,

    @ColumnInfo(name = "route_headsign")
    val routeHeadsign: String = "",

    @ColumnInfo(name = "bus_id")
    val busId: Int = 0,

    @ColumnInfo(name = "start_stop")
    val startStop: Int = 0,

    @ColumnInfo(name = "end_stop")
    val endStop: Int = 0,
) {
    // For comparing the content of trips, ignores: id, startTime, and endTime
    fun equals(other: Trip): Boolean {
        return this.routeId == other.routeId
                && this.routeHeadsign == other.routeHeadsign
                && this.busId == other.busId
                && this.startStop == other.startStop
                && this.endStop == other.endStop
    }

    fun isEmpty(): Boolean {
        return this.equals(defaultTrip)
    }

    companion object {
        val defaultTrip = Trip(startTime = 0L)
    }
}