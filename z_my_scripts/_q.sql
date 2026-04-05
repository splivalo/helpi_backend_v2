-- Clear existing admin notifications and seed 12 visible admin types
DELETE FROM "HNotifications" WHERE "RecieverUserId" = 1;

INSERT INTO "HNotifications"
  ("RecieverUserId","Type","TranslationKey","Title","Body","IsRead","CreatedAt","StudentId","SeniorId","OrderId","OrderScheduleId","JobInstanceId","Payload")
VALUES
  -- GREEN: Added
  (1, 24, 'Notifications.NewStudent', 'Novi student dodan', 'Luka Peric', false, NOW() - INTERVAL '1 minute', 101, NULL, NULL, NULL, NULL, '{"studentId":101}'),
  (1, 25, 'Notifications.NewSenior',  'Novi senior dodan',  'Ivka Mandic', false, NOW() - INTERVAL '2 minutes', NULL, 1, NULL, NULL, NULL, '{"seniorId":1}'),
  (1, 30, 'Notifications.NewOrder',   'Nova narudzba kreirana', 'Ivka Mandic, Narudzba #3', false, NOW() - INTERVAL '3 minutes', NULL, 1, 3, NULL, NULL, '{"orderId":3,"seniorId":1}'),

  -- RED: Cancelled
  (1, 12, 'Notifications.OrderCancelled',              'Narudzba otkazana', 'Marija Horvat, Narudzba #5', false, NOW() - INTERVAL '4 minutes', NULL, 2, 5, NULL, NULL, '{"orderId":5,"seniorId":2}'),
  (1, 10, 'Notifications.ScheduleAssignmentCancelled', 'Termin otkazan',    'Ivka Mandic, Narudzba #3',   false, NOW() - INTERVAL '5 minutes', 101, 1, 3, NULL, NULL, '{"orderId":3,"studentId":101}'),
  (1,  8, 'Notifications.JobCancelled',                'Posjet otkazan',    'Ivka Mandic, Narudzba #1',   false, NOW() - INTERVAL '6 minutes', NULL, 1, 1, 1, 10, '{"jobInstanceId":10,"orderId":1}'),

  -- RED: Deleted
  (1, 26, 'Notifications.UserDeleted', 'Student izbrisan',  'Student: Ivan Babic (ID: 102) je trajno izbrisan', false, NOW() - INTERVAL '7 minutes', 102, NULL, NULL, NULL, NULL, '{"deletedUserId":102,"deletedUserName":"Ivan Babic","userType":"StudentDeleted"}'),
  (1, 27, 'Notifications.UserDeleted', 'Senior izbrisan',   'Starija osoba: Petar Novak (ID: 2) je trajno izbrisan', false, NOW() - INTERVAL '8 minutes', NULL, 2, NULL, NULL, NULL, '{"deletedUserId":2,"deletedUserName":"Petar Novak","userType":"SeniorDeleted"}'),

  -- ORANGE: Changes / Warning
  (1, 31, 'Notifications.AvailabilityChanged', 'Promjena dostupnosti studenta', 'Ana Matic, Pogodena narudzba #3 (Pon 09:00-11:00)', false, NOW() - INTERVAL '9 minutes', 102, NULL, 3, NULL, NULL, '{"studentName":"Ana Matic","orderId":3,"scheduleDescription":"Pon 09:00-11:00"}');

SELECT "Id","Type","Title","Body" FROM "HNotifications" WHERE "RecieverUserId"=1 ORDER BY "CreatedAt" DESC;

SELECT "Id", "Type", "Title", "TranslationKey" FROM "HNotifications" WHERE "RecieverUserId" = 1 ORDER BY "CreatedAt";
