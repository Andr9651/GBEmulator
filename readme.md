## Things I would do differently
* CPU
    * Group the instructions by type instead of ordering them by Opcode.\
    Doing it this way resulted in a lot of jumping around when implementing them.\
    It also resulted in alot of missed opportunities for code deduplication on the initial implementation that was then later changed.
