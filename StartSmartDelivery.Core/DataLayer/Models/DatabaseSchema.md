```mermaid
erDiagram
    Driver {
        int Id PK
        string Name
        string Surname
        string EmployeeNo
        LicenseType LicenseType
    }

    Vehicle {
        int Id PK
        string Make
        string Model
        int Year
        string NumberPlate
        bool Availability
    }

   DeliveryTask {
        int Id PK
        string OrderNumber
        string CustomerCode
        string Name
        string Telephone 
        string Cellphone
        string Email     
        string Address
        string Product
        decimal Amount
        PaymentMethod PaymentMethod
        string Notes
        DateTimeOffset ReceivedTimestamp
    }

    Delivery {
        int Id PK
        int TaskId FK
        int DriverId FK
        int VehicleId FK
        DateTimeOffset DispatchTimestamp
    }

    Driver ||--o{ Delivery : "performs"
    Vehicle ||--o{ Delivery : "assigned to"
    DeliveryTask ||--o{ Delivery : "fulfilled by"
```
