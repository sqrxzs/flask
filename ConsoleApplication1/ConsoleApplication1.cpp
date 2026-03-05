#define _CRT_SECURE_NO_WARNINGS
#include <iostream>
#include <cstring>


using namespace std;


const int MAX_USERS = 10;
const int MAX_CHATS = 10;
const int MAX_MESSAGES_PER_CHAT = 50;
const int MAX_STRING = 50;


struct Message {
    char sender[MAX_STRING];
    char text[MAX_STRING];
};


struct Chat {
    char name[MAX_STRING];
    char participants[MAX_USERS][MAX_STRING]; 
    int participantCount;
    Message messages[MAX_MESSAGES_PER_CHAT];
    int messageCount;
};


char userNames[MAX_USERS][MAX_STRING];
char userPasswords[MAX_USERS][MAX_STRING];
int userCount = 0;

Chat chats[MAX_CHATS];
int chatCount = 0;

int currentUserIndex = -1;  // индекс текущего вошедшего пользователя (-1 = никто)


int findUser(const char* name) {
    for (int i = 0; i < userCount; ++i) {
        if (strcmp(userNames[i], name) == 0)
            return i;
    }
    return -1;
}


void registerUser(const char* name, const char* pass) {
    if (userCount >= MAX_USERS) {
        cout << "Лимит пользователей исчерпан.\n";
        return;
    }
    if (findUser(name) != -1) {
        cout << "Пользователь с таким именем уже существует.\n";
        return;
    }
    strcpy(userNames[userCount], name);
    strcpy(userPasswords[userCount], pass);
    userCount++;
    cout << "Пользователь " << name << " зарегистрирован.\n";
}


bool login(const char* name, const char* pass) {
    int idx = findUser(name);
    if (idx != -1 && strcmp(userPasswords[idx], pass) == 0) {
        currentUserIndex = idx;
        cout << "Добро пожаловать, " << name << "!\n";
        return true;
    }
    cout << "Неверное имя или пароль.\n";
    return false;
}


void logout() {
    currentUserIndex = -1;
    cout << "Вы вышли из системы.\n";
}


void createChat(const char* chatName, const char* participantsList[], int partCount) {
    if (chatCount >= MAX_CHATS) {
        cout << "Лимит чатов исчерпан.\n";
        return;
    }
    Chat& newChat = chats[chatCount];
    strcpy(newChat.name, chatName);
    newChat.participantCount = partCount;
    for (int i = 0; i < partCount; ++i) {
        strcpy(newChat.participants[i], participantsList[i]);
    }
    newChat.messageCount = 0;
    chatCount++;
    cout << "Чат \"" << chatName << "\" создан.\n";
}

void sendMessage(int chatIdx, const char* text) {
    if (currentUserIndex == -1) {
        cout << "Ошибка: необходимо войти в систему.\n";
        return;
    }
    if (chatIdx < 0 || chatIdx >= chatCount) {
        cout << "Неверный индекс чата.\n";
        return;
    }
    Chat& chat = chats[chatIdx];
    if (chat.messageCount >= MAX_MESSAGES_PER_CHAT) {
        cout << "В этом чате слишком много сообщений.\n";
        return;
    }
    Message& msg = chat.messages[chat.messageCount];
    strcpy(msg.sender, userNames[currentUserIndex]);
    strcpy(msg.text, text);
    chat.messageCount++;
    cout << "Сообщение отправлено.\n";
}


void showMyChats() {
    if (currentUserIndex == -1) {
        cout << "Ошибка: необходимо войти в систему.\n";
        return;
    }
    cout << "Ваши чаты:\n";
    for (int i = 0; i < chatCount; ++i) {
        Chat& chat = chats[i];
        for (int j = 0; j < chat.participantCount; ++j) {
            if (strcmp(chat.participants[j], userNames[currentUserIndex]) == 0) {
                cout << i << ": " << chat.name << "\n";
                break;
            }
        }
    }
}


void viewChat(int chatIdx) {
    if (currentUserIndex == -1) {
        cout << "Ошибка: необходимо войти в систему.\n";
        return;
    }
    if (chatIdx < 0 || chatIdx >= chatCount) {
        cout << "Неверный индекс чата.\n";
        return;
    }
    Chat& chat = chats[chatIdx];
    cout << "Чат \"" << chat.name << "\":\n";
    for (int i = 0; i < chat.messageCount; ++i) {
        Message& msg = chat.messages[i];
        cout << "[" << msg.sender << "]: " << msg.text << "\n";
    }
}


void parseCommand(const char* input, char* cmd, char* args[10], int& argCount) {
    argCount = 0;
    char temp[256];
    strcpy(temp, input);
    char* token = strtok(temp, " ");
    if (token) {
        strcpy(cmd, token);
        while ((token = strtok(nullptr, " ")) && argCount < 9) {
            args[argCount] = new char[MAX_STRING];
            strcpy(args[argCount], token);
            argCount++;
        }
    }
}


void freeArgs(char* args[], int argCount) {
    for (int i = 0; i < argCount; ++i)
        delete[] args[i];
}

int main() {
    setlocale(LC_ALL, "Russian");

    registerUser("alice", "pass");
    registerUser("bob", "pass");
    
    cout << "\n--- Добро пожаловать в корпоративной мессенджер ---\n";
    cout << "Команды:\n";
    cout << "  login <имя> <пароль>             - вход\n";
    cout << "  logout                           - выход\n";
    cout << "  createchat <название> [участники...] - создать чат\n";
    cout << "  mychats                           - список ваших чатов\n";
    cout << "  send <индекс_чата> <текст>        - отправить сообщение\n";
    cout << "  view <индекс_чата>                 - просмотреть сообщения\n";
    cout << "  exit                               - выход\n";

    while (true) {
        cout << "\n> ";
        char input[256];
        cin.getline(input, 256);

        char cmd[50] = "";
        char* args[10] = { nullptr };
        int argCount = 0;
        parseCommand(input, cmd, args, argCount);

        if (strcmp(cmd, "exit") == 0) {
            freeArgs(args, argCount);
            break;
        }
        else if (strcmp(cmd, "login") == 0 && argCount >= 2) {
            login(args[0], args[1]);
        }
        else if (strcmp(cmd, "logout") == 0) {
            logout();
        }
        else if (strcmp(cmd, "createchat") == 0 && argCount >= 1) {

            const char* participantsList[10];
            int partCount = argCount - 1;
            for (int i = 0; i < partCount; ++i)
                participantsList[i] = args[i + 1];
            createChat(args[0], participantsList, partCount);
        }
        else if (strcmp(cmd, "mychats") == 0) {
            showMyChats();
        }
        else if (strcmp(cmd, "send") == 0 && argCount >= 2) {
            int idx = atoi(args[0]); 

            char text[200] = "";
            for (int i = 1; i < argCount; ++i) {
                if (i > 1) strcat(text, " ");
                strcat(text, args[i]);
            }
            sendMessage(idx, text);
        }
        else if (strcmp(cmd, "view") == 0 && argCount >= 1) {
            int idx = atoi(args[0]);
            viewChat(idx);
        }
        else {
            cout << "Неизвестная команда или не хватает аргументов.\n";
        }

        freeArgs(args, argCount);
    }

    return 0;
}