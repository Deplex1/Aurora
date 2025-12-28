-- הגירה למערכת אמנים מרובים - יצירת טבלת song_artists והעברת נתונים

-- 1. יצירת הטבלה החדשה
CREATE TABLE song_artists (
    songid INT NOT NULL,
    userid INT NOT NULL,
    role VARCHAR(50) DEFAULT 'main',
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (songid, userid),

    CONSTRAINT fk_song_artists_song
        FOREIGN KEY (songid) REFERENCES songs(songid)
        ON DELETE CASCADE,

    CONSTRAINT fk_song_artists_user
        FOREIGN KEY (userid) REFERENCES users(userid)
        ON DELETE CASCADE
);

-- 2. יצירת אינדקסים לביצועים טובים
CREATE INDEX idx_song_artists_songid ON song_artists(songid);
CREATE INDEX idx_song_artists_userid ON song_artists(userid);
CREATE INDEX idx_song_artists_role ON song_artists(role);

-- 3. העברת הנתונים הקיימים (כל שיר מקבל את האמן שהעלה אותו כ-main artist)
INSERT INTO song_artists (songid, userid, role)
SELECT songid, userid, 'main'
FROM songs
WHERE userid IS NOT NULL;

-- 4. בדיקה - כמה רשומות הועברו
SELECT 'Migration completed. Records migrated:' as status, COUNT(*) as count
FROM song_artists;

-- 5. אופציונלי: ניתן להסיר את userid מ-songs אם הוא כבר לא נחוץ
-- ALTER TABLE songs DROP COLUMN userid;

-- הערות:
-- * כל שיר קיים יקבל את המשתמש שהעלה אותו כ-main artist
-- * ניתן להוסיף אמנים נוספים בעזרת הממשק החדש
-- * המערכת תומכת עכשיו באמנים מרובים לכל שיר
