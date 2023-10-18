public class Post {
    public long id;
    public string name;
    public string email;
    public string imageURL;
    public string content;
    public string dateTime;
    public long like_counts = 0;
    public long download_counts = 0;

    public Post() {
    }

    public Post(long id, string name, string email, string imageURL, string content, string dateTime) {
        this.id = id;
        this.name = name;
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
        this.dateTime = dateTime;
    }

    public Post(long id, string name, string email, string imageURL, string content, string dateTime, long like_counts, long download_counts) {
        this.id = id;
        this.name = name;
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
        this.dateTime = dateTime;
        this.like_counts = like_counts;
        this.download_counts = download_counts;
    }

    public Post(long id, string name, string email, string imageURL, string content, string dateTime, long download_counts) {
        this.id = id;
        this.name = name;
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
        this.dateTime = dateTime;
        this.download_counts = download_counts;
    }
}