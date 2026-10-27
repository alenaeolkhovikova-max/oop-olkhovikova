#include "todolist.h" 

#include <fstream>
#include <sstream>
#include <iostream>
using namespace std;

void TodoList::Add(const string& title, const string& date) {
    auto item = make_shared<TodoItem>(title, date);
    _items.push_back(item);
}

void TodoList::Remove(size_t index) {
    if (index < _items.size()) {
        _items.erase(_items.begin() + index);
    }
}

void TodoList::ToggleDone(size_t index) {
    if (index < _items.size()) {
        _items[index]->Complete();
    }
}

void TodoList::Rename(size_t index, const string& newTitle) {
    if (index < _items.size()) {
        _items[index]->Rename(newTitle);
    }
}

shared_ptr<TodoItem> TodoList::GetItem(size_t index) const {
    if (index < _items.size()) return _items[index];
    return nullptr;
}


void TodoList::SaveToFile() {
    ofstream file(_filename);
    if (!file.is_open()) return;
    
    file << "[\n";
    for (size_t i = 0; i < _items.size(); ++i) {
        const auto& item = _items[i];
        file << "  {\n";
        file << "    \"title\": \"" << item->Title() << "\",\n";
        file << "    \"done\": " << (item->IsDone() ? "true" : "false") << ",\n";
        file << "    \"date\": \"" << item->DueDate() << "\"\n";
        file << "  }";
        if (i < _items.size() - 1) file << ",";
        file << "\n";
    }
    file << "]\n";
    file.close();
}

void TodoList::LoadFromFile() {
    ifstream file(_filename);
    if (!file.is_open()) return;
    
    string line;
    string currentTitle, currentDate;
    bool currentDone = false;
    bool inObject = false;
    
    while (getline(file, line)) {
        if (line.find('{') != string::npos) {
            inObject = true;
            currentTitle.clear(); currentDate.clear(); currentDone = false;
        } else if (inObject && line.find("\"title\"") != string::npos) {
            size_t start = line.find(':');
            size_t qStart = line.find('"', start + 1);
            size_t qEnd = line.rfind('"');
            if (qStart != string::npos && qEnd > qStart) 
                currentTitle = line.substr(qStart + 1, qEnd - qStart - 1);
        } else if (inObject && line.find("\"done\"") != string::npos) {
            currentDone = (line.find("true") != string::npos);
        } else if (inObject && line.find("\"date\"") != string::npos) {
            size_t start = line.find(':');
            size_t qStart = line.find('"', start + 1);
            size_t qEnd = line.rfind('"');
            if (qStart != string::npos && qEnd > qStart) 
                currentDate = line.substr(qStart + 1, qEnd - qStart - 1);
        } else if (line.find('}') != string::npos && inObject) {
            if (!currentTitle.empty()) {
                auto item = make_shared<TodoItem>(currentTitle, currentDate);
                if (currentDone) item->Complete();
                _items.push_back(item);
            }
            inObject = false;
        }
    }
    file.close();
}
