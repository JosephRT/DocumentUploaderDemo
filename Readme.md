# Document Upload Demo

A small project that demonstrates two ways of uploading files to a ASP.NET Core REST API.  It allows users to POST files via the POST body or via a multipart form, list all of the file's metadata, download uploaded files, or delete them.  SQLite is used to store the documents.

Note the project only supports test (txt) files at the moment, for security reasons.

## Getting Started

### Prerequisites
To run the demo, users need

* Visual Studio 2019
* .NET Core 3.1
* ASP.NET Core

## Running the tests

### Unit Tests
The unit tests are build with NUnit and Moq.  They can all be ran via any standard test runner (Visual Studio's build in runner, ReSharper's runner, etc) without any additional configuration.

### End-to-End/Integration Tests
End-to-end/integration tests have yet to be implemented.

## Running the Demo

### Starting the Service
1. Open the solution in Visual Studio 2019
2. Ensure that `DocumentUploadApi` is marked as the start up project
3. Hit `Run`

### Endpoints
The API exposes the following endpoints:

* DELETE `/api/files/{documentId}` deletes the document specified under the specified ID.  Note that it's idempotent; subsequent calls to delete the same document will not change the system's state.
* GET `/api/files/` returns a list of all of the uploaded file's metadata, or else an empty list.
* GET `/api/files/{documentId}` returns the file saved under the specified ID, or else `404 - Not Found` if a document with the specified ID doesn't exist.
* POST `/api/files/` accepts files to be saved.  Malformed requests will receive a response of `400 - Bad Request`.  There are two supported methods to upload files:
    - Large file uploads completed via a multipart form request.
    - File binary data included in the POST body.  Note that this call requires a `fileName` query parameter, as the file name is not included with the POST body.

## Architecture
The project's architecture is divided into three main components, minus the projects that host the unit tests.  They are:
* The core library (`DocumentUploadCore`)
* The data source (`SQLiteDataSource`)
* The exposed REST API (`DocumentUploadApi`)

### Core Library's Considerations and Overview
Despite being a bit boring at the moment, the Core library is the driver of the actual business logic.  It's responsible for processing managed documents; it's not responsible for storage implementation nor parsing incoming documents from various sources.

It is, however, responsible for defining what those sources and storage implementations need to do.  It accomplishes this via the dependency inversion principle to expose a repository interface for providers to implement; in the case of this project, `SQLiteDataSource` uses this interface to provide a SQLite implementation which can referenced at runtime.  The inversion gives the business logic full control over shape of the data store's interface, instead of being driven by it.

Also of note, is that the Core library is completely devoid of Nuget packages; it's pure C# code.  This is by design; I do my best to keep my business logic code as dependency free as possible, which provides maximum flexibility and minimum fighting with external packages.  The business logic should be purely on the developer's terms, and not subject to external packages' whims.  I'm not offended by using Nuget packages, but I do think that they should be as contained as possible so when breaking changes roll in via updates there is a minimum of code that needs to be addressed.

### SQLiteDataSource's Considerations and Overview
Another simple project, it contains a SQLite implementation of the data repository that the Core library defined.  The actual queries were implemented with Dapper for support.

Dapper was chosen due to it's simplicity in mapping SQL queries to objects.  I wanted to execute SQL directly, but without the ceremony of all of the manual mapping.  Also note that it doesn't reference any of the other projects in solution other than the repository interface.  This is intended to keep the coupling as loose as possible.

Of note, there are no unit tests for the SQLite project.  The project exclusively interacts with a provided database; I usually lean on integration tests for exercising external dependencies such as this.   In this case, SQLite could release an update that makes unit tests obsolete and useless; the only way we'd know if it's an issue is to run our code against the update.

### API's Consideration and Overview
The API project, being the point of execution, is a bit more complex.  Being the point of execution, it's where dependencies are decided and defined.  I opted for ASP.NET Core's build-in dependency injection framework for resolving the various dependencies.  I made the decision due to it's general simplicity and documentation; I neither need nor want a more complicated tool, and it's features are easily Google-able.  As such, it has references to both projects to resolve their respective provided identities as well as a Nuget package for SQLite connection support.

It's worth stating that I don't necessarily think that a dependency injection framework is always warranted; I've personally experienced circumstances where the DI framework was far more of a hindrance to understanding rather than a boon.  I don't quite believe that, to quote a friend, that "dependency injection frameworks exist to confuse you".  Used carefully they have use, and the simplicty and ubiquity of ASP.NET Core's framework was useful enough for me to take advantage of.

The REST API itself was designed with simplicity and usability in mind.  The DELETE endpoint was fairly straightforward; it should do what it says on the tin.  PUT was excluded intentionally; for simplicity's sake I choose to make the uploaded documents idempotent.  GET-ing a specific document via it's ID also seemed obvious; return the file itself for download.  The GET endpoint to list documents and the POST endpoints warranted a bit of extra thought, though.

Listing all REST documents is usually a straight-forward affair: retrieve all of the (typically JSON, in my case) documents and return them in a JSON array.  Binary files presented a bit of a problem, as "returning the documents" meant literally returning all of the documents for downloading at once, which didn't make sense.  I opted to instead return a list of the document's metadata, which gives the end user enough information to make an informed choice as to what they'd like to do next.  I supposed this in code by grouping all of the document's metadata attributes into their own file.  I find that passing around object with `null` properties can often get confusing; is it intentionally `null`, or is that a bug?  I try to avoid those types of cases where possible, as I did here.

POST-ing documents presented a slightly different challenge as there are multiple ways of sending documents via HTTP: a multipart form or directly submitting the binary via the POST body.  From the controller's perspective, however, it generally doesn't care; it needs to process files the same way regardless of how they came in.  This lead to my current approach; classes that are dedicated to handling different types of POST-ed documents and a factory that determines which type of POST the request is.  This encapsulates each type of requests somewhat complicated logic in their own area, leaving the controller relatively clean and easily testable.

As an added bonus, this also keeps the API simple by providing a single endpoint to create new documents, regardless of how they're submitted.  I personally dislike "similar but different" things along those lines; if there were two different endpoints depending on how you were submitting your file users would have to keep the differences between them in their head.  One point per action, in my experience, greatly reduces friction and irritation when using interfaces.

Also of note is the decoupling of the individual components while keeping their external dependencies to a fairly reasonable number (the greatest is the API's controller, with three).  I make a concentrated effort to make my code as convenient to test as possible.  I've found that convenient to test code also results in tests that are easier to maintain, so changes are less painful than when you have to maintain ten plus dependencies along with a hundred lines of test set up.

A potential point of confusion is that the two classes that process the different types of POST requests aren't really unit tested.  I sometimes leave these for integration tests.  There are times where the setup necessary for potentially complicated input like this can be a bit tedious, leading to tests that are not necessarily the clearest nor terribly well maintained.  In this case, instead of manually setting up a request object it'd be easier to create an integration test that actually sent a request to the end to see what happens.

These two classe also do input validation as it pertains to the HTTP request, but note that it doesn't validate the file itself.  I've made a distinction on the types of validation; the API endpoint is the HTTP interface, so it should validate the input as it relates to HTTP; whether a file has been provided, whether it's too large for the application to accept, and so on.  The type and actual contents, in my view, should be delegated to the Core library.  It's up to the API to ensure that what's been provided is valid as far as HTTP is concerned, it's on the library to determine whether it can accept it and whether it's contents are valid.

## Future Plans/To-Do List
* Integration tests - there currently isn't any end-to-end tests to thoroughly test the input parsing as well as the persistence
* In-depth file specific validation - The Core library currently only supports text files, and it does that by looking at the file extension.  This could be improved by implementing validators for all of the types of files that should be supported to ensure that the file is what the extension says it is.
* Auth - the API currently supports neither authentication nor authorization, which would be nice to have.
* Dockerization - while this API was created deliberately without Docker for the sake of being able to run it without having Docker installed, it'd be a nice to have.
* Database migrations - as a way to get going, the provided SQLite database is simple and already set up.  I'd be nice to have database versioning in place for future enhancements.