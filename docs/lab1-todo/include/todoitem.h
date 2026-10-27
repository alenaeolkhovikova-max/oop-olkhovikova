#ifndef TODOITEM_H
#define TODOITEM_H

#include <string>
#include <stdexcept>
using namespace std;

class TodoItem {
private:
    string _title;
    bool _isDone;
    string _dueDate; 

public:
    TodoItem(const string& title, const string& dueDate = "") 
        : _isDone(false), _dueDate(dueDate) 
    {
        Rename(title); 
    }

    const string& Title() const { return _title; }
    bool IsDone() const { return _isDone; }
    const string& DueDate() const { return _dueDate; }

    
    void Complete() { _isDone = true; }
    
    void Rename(const string& newTitle) {
        if (newTitle.empty()) {
            throw invalid_argument("Название задачи не может быть пустым");
        }
        _title = newTitle;
    }

    void SetDueDate(const string& date) {
        _dueDate = date;
    }
};

#endif
