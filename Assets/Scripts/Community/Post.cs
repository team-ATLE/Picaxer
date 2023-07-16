public class Post {
    public string email;
    public string imageURL;
    public string content;
    public string dateTime;

    public Post() {
    }

    public Post(string email, string imageURL, string content, string dateTime) {
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
        this.dateTime = dateTime;
    }
}