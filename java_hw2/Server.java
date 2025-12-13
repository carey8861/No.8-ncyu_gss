import java.io.*;
import java.net.*;

public class Server {
    public static void main(String[] args) {
        try {
            ServerSocket serverSocket = new ServerSocket(5000);
            System.out.println("伺服器已啟動，等待連線中...");

            Socket socket = serverSocket.accept();
            System.out.println("客戶端已連線！");

            
            BufferedReader input = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));
            PrintWriter output = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), "UTF-8"), true);
            BufferedReader console = new BufferedReader(new InputStreamReader(System.in, "UTF-8"));

            String message;
            while (true) {
                message = input.readLine();
                if (message == null || message.equalsIgnoreCase("exit")) {
                    System.out.println("客戶端離線。");
                    break;
                }
                System.out.println("Client: " + message);

                System.out.print("Server: ");
                String reply = console.readLine();
                output.println(reply);
                if (reply.equalsIgnoreCase("exit")) break;
            }

            socket.close();
            serverSocket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}

