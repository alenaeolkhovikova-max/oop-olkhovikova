#include <iostream>
#include <string>

#include "../include/todolist.h" 
using namespace std;

int main() {
    TodoList list("tasks.json");
    list.LoadFromFile();
    
    int choice;
    do {
        cout << "\n=== Мои задачи ===\n";
        cout << "1. Показать все\n2. Добавить\n3. Выполнить\n4. Переименовать\n5. Удалить\n6. Выход\nВыбор: ";
        
        if (!(cin >> choice)) break;
        cin.ignore();
        
        try {
            switch (choice) {
                case 1: {
                    if (list.Count() == 0) cout << "Нет задач\n";
                    else {
                        for (size_t i = 0; i < list.Count(); ++i) {
                            auto item = list.GetItem(i);
                            cout << i+1 << ". [" << (item->IsDone()?"X":" ") << "] " 
                                      << item->Title();
                            if (!item->DueDate().empty()) 
                                cout << " (до " << item->DueDate() << ")";
                            cout << "\n";
                        }
                    }
                    break;
                }
                case 2: {
                    string title, date;
                    cout << "Название: "; getline(cin, title);
                    cout << "Срок (Enter для пропуска): "; getline(cin, date);
                    list.Add(title, date);
                    list.SaveToFile();
                    cout << "Добавлено!\n";
                    break;
                }
                case 3: {
                    size_t idx; cout << "Номер: "; 
                    if (cin >> idx && idx >= 1 && idx <= list.Count()) {
                        list.ToggleDone(idx-1); list.SaveToFile();
                        cout << "Отмечено!\n";
                    }
                    break;
                }
                case 4: {
                    size_t idx; cout << "Номер: ";
                    if (cin >> idx && idx >= 1 && idx <= list.Count()) {
                        cin.ignore(); string newT;
                        cout << "Новое название: "; getline(cin, newT);
                        list.Rename(idx-1, newT); list.SaveToFile();
                        std::cout << "Переименовано!\n";
                    }
                    break;
                }
                case 5: {
                    size_t idx; cout << "Номер: ";
                    if (cin >> idx && idx >= 1 && idx <= list.Count()) {
                        list.Remove(idx-1); list.SaveToFile();
                        cout << "Удалено!\n";
                    }
                    break;
                }
            }
        } catch (const exception& e) {
            cerr << "Ошибка: " << e.what() << "\n";
        }
    } while (choice != 6);
    
    return 0;
}
