public class Post {
    public string email;
    public string imageURL;
    public string content;

    public Post() {
    }

    public Post(string email, string imageURL, string content) {
        this.email = email;
        this.imageURL = imageURL;
        this.content = content;
    }
}