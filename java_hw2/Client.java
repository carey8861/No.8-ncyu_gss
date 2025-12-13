import java.io.*;
import java.net.*;

public class Client {
    public static void main(String[] args) {
        try {
            BufferedReader console = new BufferedReader(new InputStreamReader(System.in, "UTF-8"));
            System.out.print("請輸入伺服器 IP（例如 localhost）: ");
            String serverIP = console.readLine();

            Socket socket = new Socket(serverIP, 5000);
            System.out.println("已連線至伺服器！");

            BufferedReader input = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));
            PrintWriter output = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), "UTF-8"), true);

            String message;
            while (true) {
                System.out.print("Client: ");
                String sendMsg = console.readLine();
                output.println(sendMsg);
                if (sendMsg.equalsIgnoreCase("exit")) break;

                message = input.readLine();
                if (message == null || message.equalsIgnoreCase("exit")) {
                    System.out.println("伺服器結束連線。");
                    break;
                }
                System.out.println("Server: " + message);
            }

            socket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}

