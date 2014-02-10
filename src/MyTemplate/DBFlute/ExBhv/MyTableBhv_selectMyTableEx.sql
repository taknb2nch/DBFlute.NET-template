-- #df:entity#
-- !df:pmb!
-- !!int? age!!
-- !!string attendanceFlagTrue:cls(Flag.True)!!
select 
  * 

from my_table A 

where 
  A.age > /*pmb.Age*/50 
  and A.attendance_flag = /*pmb.AttendanceFlagTrue*/'0'

order by A.id asc
