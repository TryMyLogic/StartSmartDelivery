# StartSmartDeliveryManagementSystem

## Note: 
> Development on this project is temporarily postponed while I focus on a long-term university project that has begun in May, involving a Braille display with IoT integration.  
> Work will resume after its completion.

## Description
The goal of this project is to resolve an issue encountered by Tiago Luiz in a real-world business.
The business currently tracks deliveries by writing them manually on a whiteboard using a whiteboard marker. 
This method presents several challenges that the project aims to resolve.

## Problem Statement
* Lack of Digital Record: Whiteboards can be easily erased, potentially resulting in loss of important information on upcoming deliveries.
Limited Scalability:  As the business grows, tracking delieveries on a whiteboard becomes impractical. As a result of space constraints, only
a certain number of employees can crowd the board to read or write at a time. In addition, even the largest whiteboards have limited space for writing 
which can vary depending on how large and skew an employee writes. 
* Limited Access and Visibility: As the business grows, if two more more seperate sites are opened, teams cannot collaborate and share information in one 
centralized location.
* Lack of Real-Time Updates: As the whiteboard is a physical item, the system does not have the potential to update in real-time
across multiple locations should the company require it. This can result in information flow delays and the potential for miscommunication.
* Tracking and Reporting Challenges: Completed devliveries are erased from the whiteboard, preventing a company from attempting to review past performance and 
analyze trends, potentially discovering patterns that may result in a competitive advantage.
* Customer Information Duplication: Customers may phone in with a new name and phone number, but for an existing location. 
This makes it making it difficult to consolidate and manage customer information effectively.
* Inconsistent Data Entry: How information is recorded differs between employees as a result of factors like handwriting legibility, knowledge on abbreviations etc. 
This may cause confusion or misinterpretation.
* Difficulty in Prioritizing Deliveries: As data is not sortable and clearly represented on a whiteboard, urgent delieveries are more challenging to prioritize.
* Limited Accountability and Traceability: Whiteboards do not track which employee handles which delievery, resulting in a lack of accountability if issues arise.
* Lack of intergration with other business systems: Manual track is isolated and cannot integrate with other systems such as Custer Relationship Management (CRM)
and inventory management. As a result, duplication of work can occur, especially when cross-referencing data.

## Technologies Used
- **Frontend**: Winforms (.NET)
- **Backend**: .NET #8
- **Database**: Sql Server Express

## Installation
TODO (When version 1 is released)

## Usage
TODO (When version 1 is released)

## Project Background
StartSmartDeliever was originally conceived by Tiago Luiz and developed as a university project of the same name. This repository refines and expands the concept into one that can be applied in the real world. The original university project can be found [here](https://github.com/zannlin/StartSmartDelivery).

## Contributing
* Tiago Luiz - https://www.linkedin.com/in/tiago-luiz-8192a8358/
* Zander Lindeque - https://www.linkedin.com/in/zander-lindeque-b2aa42268/
* Tyler Geuens - https://www.linkedin.com/in/tyler-geuens-512566273/
* Reghard Du Plessis - https://www.linkedin.com/in/reghard-du-plessis-a63753285/

## Planned Features
# For V2:
* Distributed computing - multiple employees in different locations to access and input data into a centralized database
* Authentication and Auditing - Tracks which employee performed specific tasks, allowing for the recognition of high performers and ensuring unauthorized attempts can be traced for security purposes.
