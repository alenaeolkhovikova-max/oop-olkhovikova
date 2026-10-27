#ifndef TODOLIST_H
#define TODOLIST_H

#include <vector>
#include <memory>
#include <string>
#include "todoitem.h"
using namespace std;

class TodoList {
private:
    vector<shared_ptr<TodoItem>> _items;
    string _filename;

public:
    TodoList(const string& filename = "tasks.json") 
        : _filename(filename) {}
    
    
    void Add(const string& title, const string& date = "");
    void Remove(size_t index);
    void ToggleDone(size_t index);
    void Rename(size_t index, const string& newTitle);
    
    
    size_t Count() const { return _items.size(); }
    shared_ptr<TodoItem> GetItem(size_t index) const;

    
    void SaveToFile();
    void LoadFromFile();
};

#endif 
