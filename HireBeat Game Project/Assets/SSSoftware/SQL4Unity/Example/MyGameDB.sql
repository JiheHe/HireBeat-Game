TRUNCATE Players;
INSERT INTO Players (id,title,active,prefab,currpos) VALUES (10,'Cube',True,{NAME:Cube},NULL);
INSERT INTO Players (id,title,active,prefab,currpos) VALUES (20,'Sphere',True,{NAME:Sphere},NULL);
INSERT INTO Players (id,title,active,prefab,currpos) VALUES (30,'Capsule',True,{NAME:Capsule},NULL);
INSERT INTO Players (id,title,active,prefab,currpos) VALUES (40,'Cylinder',True,{NAME:Cylinder},NULL);
TRUNCATE Positions;
INSERT INTO Positions (id,position) VALUES (0,{1.0, 1.0, 1.0});
INSERT INTO Positions (id,position) VALUES (1,{2.0, 2.0, 2.0});
INSERT INTO Positions (id,position) VALUES (2,{3.0, 1.5, 3.0});
INSERT INTO Positions (id,position) VALUES (3,{4.0, 0.5, 4.0});
TRUNCATE PlayerPos;
INSERT INTO PlayerPos (playerid,posid) VALUES (10,0);
INSERT INTO PlayerPos (playerid,posid) VALUES (20,1);
INSERT INTO PlayerPos (playerid,posid) VALUES (30,2);
INSERT INTO PlayerPos (playerid,posid) VALUES (40,3);