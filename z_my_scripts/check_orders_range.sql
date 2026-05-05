SELECT o."Id", o."Status", c."FullName" as senior_name
FROM "Orders" o
LEFT JOIN "Seniors" sr ON sr."Id" = o."SeniorId"
LEFT JOIN "ContactInfos" c ON c."Id" = sr."ContactId"
WHERE o."Id" BETWEEN 15 AND 40
ORDER BY o."Id";
