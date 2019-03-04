# Heroes in Conference

[![Build Status](https://travis-ci.com/groupkilo/heroes_in_conference.svg?branch=master)](https://travis-ci.com/groupkilo/heroes_in_conference)

# Usage

## Installation

To install the repository run:

```sh
git clone https://github.com/groupkilo/heroes_in_conference.git
```

## Server

### Requirements

- OpenJDK 8 (or equivalent)
- Maven
- MySQL
- Python 3
- Yarn

### Automatic Configuration

Running `autoconf.py` will automatically configure, build and run the server. In addition, it will build the admin panel and mount it at `/admin/`. The server will bind to port `4567` so a proxy must be set up if it is desired to be exposed on port `80` or port `443` (recommended). A number of options for updating various parts of the server are available inside `autoconf.py`. For example, to additionally configure the MySQL schema run `autoconf.py` with the additional `--update-schema` option (this will be necessary on initial set-up). To list all available options run `autoconf.py --help`.

### Manual Configuration

Manual configuration of the server is error-prone, so the automatic approach is strongly recommended. However, a description is given here for completeness.

In order for the server to work correctly, you will need create three files in your `backend` directory. These are: `facebook.properties`, `database.properties`, and `admin.properties`. All commands shown in the following section assume you are already inside the `backend` directory. Also see [manually configuring the admin panel](#administration-panel). 

#### `facebook.properties`

This file gives all the information for the server to make requests to Facebook's API. For the current version of the Facebook Graph API (v3.2 at the time of writing), the following is sufficient:

```
authorisation_url=https://www.facebook.com/v3.2/dialog/oauth
graph_url=https://graph.facebook.com/v3.2/
client_id={% your Facebook client ID %}
client_secret={% your Facebook client secret %}
```

To obtain a client ID and client secret, visit [the Facebook developer site](https://developers.facebook.com/) and create a new OAuth application.

Note that you will also have to configure your site domain and site redirect URLs to be `https://{% your domain %}/api/oauth/` and `https://{% your domain %}/api/oauth/callback/`.

#### `database.properties`

This file contains all the information the server needs to communicate with your MySQL database.

Before we can configure the database settings, we need to have a database set up. Assuming that you have already created an empty MySQL database, you should run the following:

```sh
mysql -u {% your MySQL username %} -p {% your MySQL database name %} < schema.sql
```

At this point you can also add our default achievements and content groups by running:

```sh
mysql -u {% your MySQL username %} -p {% your MySQL database name %} < default.sql
```

This will automatically configure your database to run our schema. Now you can create a `database.properties` file containing:

```
driver=com.mysql.cj.jdbc.Driver
url=mysql://{% the location of your MySQL server - typically localhost %}
database={% your MySQL datbase name%}
user={% your MySQL username %}
pass={% your MySQL password %}
```

#### `admin.properties`

This file contains a hash of the password for the administration panel.

```
passhash={% SHA-256 hash of admin password %}
```

### Manual Launch

To manually launch the server, run the following:

```sh
cd backend
mvn compile exec:java
```

Please note, `autoconf.py` will automatically start the server, and is the recommended approach to starting the server.

## Administration Panel

### Requirements

- Yarn

### Manual Configuration

The administration panel is automatically built and deployed as part of the server when using `autoconf.py`. To manually build the administration panel run:

```
cd admin_panel
yarn install
yarn build
```

The administration panel is set up to be mounted at `/admin/` on the server. This means that to deploy the administration panel correctly you must execute the following (assuming you are still in the `admin_panel` directory):

```
mkdir ../backend/static
mv build ../backend/static/admin
```

## Unity Client

### Requirements

- Unity 2018.3.2, may work with other versions
- TextMesh Pro, comes with Unity in some versions
- GUI sprite assets (`Fantasy Wooden GUI : Free` plugin)
- 3D models
- Fingers Lite plugin
- Vuforia SDK

### Setup

The scenes have been setup with temporary sprite and model assets, but these aren't online so new ones must be imported and referenced in the scenes. The Api Compatibility Level in Unity must be set to `.Net 4.x` to enable use of `dynamic` in C#, this can be set under `File > Build Settings... > Player Settings... > Other Settings`

### Customisation

#### Inventory

Adding an item to the inventory requires the following steps:

1. Create a copy of a slot object (already done up to 32 total items)
2. Increase the size of the `slotFull` component of the `Inventory` object (already done up to 32 total items)
3. Drag the new object to the `Slot` component of the `Inventory` object (already done up to 32 total items, only sprite needs to be updated)
4. In order to unlock an in the inventory, `slotFull` must be set to true at that index

#### Model Viewer

Adding a model for the model viewer requires the following steps:

1. Add the model into the scene
2. Increase the `ModelList` component of the `ModelViewer` object
3. Add the model into the list (make sure it is the right index, as it needs to be the same as its index in the inventory)

#### Network Database

The `Client` script handles all networking and the `Database` script handles all local caching and saving to disk. Neither of these use any Unity calls or have any concept of the Unity Engine. The `NetworkDatabase` script is a singleton present in every scene (through `DontDestroyOnLoad`). It combines the two scripts and implements methods to enable interaction with them - the scripts in the app shouldn't have any concept of `Client` or `Database`, only `NetworkDatabase`. In general calls to the `Client` script should be made in a separate thread and then handled in Unity by setting a flag, which is checked in an Update method and then handled.

# Feedback

Feedback and bug reports are welcome!
