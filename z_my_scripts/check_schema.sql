-- OrderSchedules columns
SELECT column_name FROM information_schema.columns WHERE table_name = 'OrderSchedules' ORDER BY ordinal_position;

-- Services columns
SELECT column_name FROM information_schema.columns WHERE table_name = 'Services' ORDER BY ordinal_position;

-- JobInstances columns
SELECT column_name FROM information_schema.columns WHERE table_name = 'JobInstances' ORDER BY ordinal_position;

-- ScheduleAssignments columns
SELECT column_name FROM information_schema.columns WHERE table_name = 'ScheduleAssignments' ORDER BY ordinal_position;
