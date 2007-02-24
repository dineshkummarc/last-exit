NOTIFY_ASSEMBLY = notify-sharp.dll

notifydir = $(pkglibdir)
notify_SCRIPTS = \
	$(NOTIFY_ASSEMBLY)

NOTIFY_SRC = 				\
		$(srcdir)/Global.cs \
		$(srcdir)/Notification.cs

if EXTERNAL_DBUS
DBUS_ASSEMBLIES = $(DBUS_LIBS)
else
DBUS_ASSEMBLIES =			\
		-r:$(top_builddir)/dbus-sharp/NDesk.DBus.dll \
		-r:$(top_builddir)/dbus-sharp/NDesk.DBus.GLib.dll
endif

all: $(NOTIFY_ASSEMBLY)

$(NOTIFY_ASSEMBLY): $(NOTIFY_SRC)
	$(MCS) -target:library -out:$@ $(DBUS_ASSEMBLIES) $(GTKSHARP_LIBS) $(NOTIFY_SRC)

EXTRA_DIST = $(NOTIFY_SRC)
DISTCLEANFILES = $(NOTIFY_ASSEMBLY) $(NOTIFY_ASSEMBLY).mdb
MAINTAINERCLEANFILES = Makefile.in

