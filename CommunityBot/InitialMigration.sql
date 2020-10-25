CREATE TABLE IF NOT EXISTS main.SavedChats (
                                               Id INT PRIMARY KEY,
                                               ExactName TEXT NOT NULL,
                                               JoinLink TEXT DEFAULT NULL
);

CREATE INDEX IF NOT EXISTS main.ExactName_desc ON SavedChats (ExactName DESC);