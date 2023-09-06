public class Post {
    public string id;
    public string email;
    public string imageURL;
    public string content;
    public string dateTime;

    public Post() {
    }

    public Post(string id, string email, string imageURL, string content, string dateTime) {
        this.id = id;
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
        this.dateTime = dateTime;
    }
}