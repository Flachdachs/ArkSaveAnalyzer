using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkTools.Driver {

    public class DBDrivers {

        private static Dictionary<string, Func<DBDriver>> DRIVERS = new Dictionary<string, Func<DBDriver>>();

        //private static URLClassLoader classLoader;

        public static void addDriver(string name, Func<DBDriver> driverSupplier) {
            DRIVERS.Add(name.ToLowerInvariant(), driverSupplier);
        }

        static DBDrivers() {
            addDriver("json", () => new JsonDriver());
        }

        public static List<string> getDriverNames() {
            return DRIVERS.Keys.ToList();
        }

        public static DBDriver getDriver(string name) {
            if (DRIVERS.TryGetValue(name.ToLowerInvariant(), out Func<DBDriver> driver)) {
                return driver();
            }

            throw new NotSupportedException("Unknown Driver " + name);
        }

        /*
        public static void discoverDrivers() {
            Uri jarDirectoryURL = DBDrivers.class.getResource("/");
            Path jarPath = Paths.get(jarDirectoryURL.toURI());
            List<URL> urls = new ArrayList<>();
            List<String> driverInitClasses = new ArrayList<>();

            try (Stream<Path> pathStream = Files.walk(jarPath, 1, FileVisitOption.FOLLOW_LINKS)) {
                Iterator<Path> pathIterator = pathStream.iterator();

                while (pathIterator.hasNext()) {
                    Path path = pathIterator.next();

                    if (!path.equals(jarPath) && path.getFileName().toString().endsWith("-ark-tools-driver.jar")) {
                        try (JarFile jarFile = new JarFile(path.toFile())) {
                            Manifest jarManifest = jarFile.getManifest();
                            String dependencies = jarManifest.getMainAttributes().getValue("Driver-Dependencies");
                            String driverInit = jarManifest.getMainAttributes().getValue("Driver-Init");
                            List<URL> loadList = new ArrayList<>();
                            loadList.add(path.toUri().toURL());
  
                            boolean dependenciesFound = true;
                            if (dependencies != null) {
                                for (String dependency: dependencies.split(",")) {
                                    Path dependencyPath = jarPath.resolve(dependency.trim());
                                    if (!Files.exists(dependencyPath)) {
                                        dependenciesFound = false;
                                        System.err.println("Warning: Driver " + path.getFileName().toString() + " requires " + dependencyPath.getFileName().toString());
                                    } else {
                                        loadList.add(dependencyPath.toUri().toURL());
                                    }
                                }
                            }
  
                            if (dependenciesFound) {
                                urls.addAll(loadList);
                                driverInitClasses.add(driverInit);
                            }
                        }
                    }
                }
            }

            classLoader = new URLClassLoader(urls.toArray(new URL[urls.size()]), DBDrivers.class.getClassLoader());
            for (String driverInit: driverInitClasses) {
                Class.forName(driverInit, true, classLoader);
            }
        }

        public static void close() {
            if (classLoader != null) {
                try {
                    classLoader.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
        */
    }

}