package SQLiteServerConnector;

# SQLiteServer Connector Module for Perl

use utf8;
use strict;
use warnings;
no warnings;
use IO::Socket;
use XML::LibXML;

$| = 1;

# Private: Variables
my $ConnectionSettings = {
	host => "localhost",
	port => 11833,
	user => "Admin",
	pass => "Admin"
};

my $Socket;

# Private: Disconnect
my $Disconnect = sub {
	my $self = shift;

	if (defined $Socket) {
		eval {
			close($Socket);
		};
		if ($@) {
		}
		undef $Socket;
	}
};

# Private: Connected
my $Connected = sub {
	my $self = shift;

	my $result = 0; # false
	
	if (defined $Socket) {
		$result = 1; # true
	} else {
		$result = 0; # false
	}
	
	return $result;
};

# Private: Connect
my $Connect = sub {
	my $self = shift;
	
	my $result = 0; # false
	my $line;
	
	eval {
		# Create socket
		$Socket = new IO::Socket::INET (
			PeerHost => $ConnectionSettings->{host},
			PeerPort => $ConnectionSettings->{port},
			Proto => 'tcp',
			Timeout => 5
		);
		die "Could not create socket: $!\n" unless $Socket;
		$Socket->autoflush;
		
		# Wait for greeting
		local $SIG{ALRM} = sub { die "Read from socket failed" };
		alarm 10; # 10sec. -> timeout
		while (defined($line = <$Socket>)) {
			$line =~ s/(\r|\n)*$//g;
			if ($line ne "SQLiteServer v1.0") {
				die("Wrong Server version detected: " . $line);
			} else {
				last;
			}
		}
		alarm 0;

		# Send login
		$line = "USER:" . $ConnectionSettings->{user} . ":" . $ConnectionSettings->{pass} . "\n";
		local $SIG{ALRM} = sub { die "Write to socket failed" };
		alarm 5; # 5sec. -> timeout
		print $Socket $line;
		alarm 0;

		# Waiting for login response
		local $SIG{ALRM} = sub { die "Read from socket failed" };
		alarm 5; # 5sec. -> timeout
		while (defined($line = <$Socket>)) {
			$line =~ s/(\r|\n)*$//g;
			if (($line ne "RIGHTS:ro") && ($line ne "RIGHTS:rw")) {
				die "Wrong permissions detected";
			} else {
			  last;
			}
		}
		alarm 0;

		$result = 1;
	};
	if ($@) {
		$Disconnect->( $self );
	}		

	return $result;	
};


# Private: Send
my $Send = sub {
	my $self = shift;
	my $ASQLQuery = shift;
	my $ANoResult = shift;

	my $result = XML::LibXML::Document->new("1.0", "utf-8");
	my @SQLQueryArr = ();
	my $i;
	my $line;	

	eval {
		if ($Connected->($self) == 0) {
			if ($Connect->($self) == 0) {
				die "Cannot connect to remote host";
			}
		}

		# Send REQUEST
		@SQLQueryArr = split "\n", $ASQLQuery;
		$line = "REQUEST:" . ($#SQLQueryArr+1) . ":" . ($ANoResult == 1 ? "1" : "0") . "\n";
		local $SIG{ALRM} = sub { die "Write to socket failed" };
		alarm 5; # 5sec. -> timeout
		print $Socket $line;
		alarm 0;
		for $i (0..$#SQLQueryArr) {
			$SQLQueryArr[$i] =~ s/(\n|\r)*//g;
			$line = "." . $SQLQueryArr[$i] . "\n";
			local $SIG{ALRM} = sub { die "Write to socket failed" };
			alarm 5; # 5sec. -> timeout
			print $Socket $line;
			alarm 0;
		}
		@SQLQueryArr = ();
		
		# Read
		if ($ANoResult == 0) { # false
			my $Buffer = "";
			my $Count = -1;
			my $RecvStr = "";
			my $RecvLine = "";
			my $RecvDone = 0; # false

			while (1) {

				local $SIG{ALRM} = sub { die "Read from socket failed" };
				alarm 5; # 5sec. -> timeout
				$RecvLine = <$Socket>;
				alarm 0;

				if ($RecvLine =~ /^\s*$/) {
					# Nothing to do
				} else {
					# RESULT
					if ($RecvLine =~ m/^RESULT\:(\d+)/i) {
						$Count = $1;
						$RecvDone = 0; # false
					# .
					} elsif (($RecvDone == 0) && ($RecvLine =~ m/^\./)) {
						$Count -= 1;
						$RecvLine =~ s/(\r|\n)*$//g;
						$RecvLine =~ s/^\.//;
						$RecvStr = $RecvStr . $RecvLine . "\n";
						if ($Count == 0) {
							$RecvStr =~ s/(\r|\n)*$//g;
							$RecvDone = 1; # true
							$Count = -1;
						}
					}

					if ($RecvDone == 1) { # true
						$result = XML::LibXML->load_xml(
							string => $RecvStr
						);
						last;
					}
             			}
			}
		}
	};
	if ($@) {
		$Disconnect->($self);

		my $root = $result->createElement("Result");
		my $status = $result->createElement("Status");
		$status->setAttribute("Error", "true");
		$status->setAttribute("ErrorMessage", $@);
		$root->appendChild($status);
		$result->setDocumentElement( $root );
	}
	
	return $result;
};

# Private: ParseSQLiteResult
my $ParseSQLiteResult = sub {
	my $self = shift;
	my $AResultRef = shift;
		
	my $root = $$AResultRef->{XML}->documentElement();

	# Error flag
	eval {
		my $Status = $root->getChildrenByTagName("Status")->[0];
		$$AResultRef->{Error} = $Status->getAttribute("Error") eq "false" ? 0 : 1;
		if ($$AResultRef->{Error} == 1) {
			$$AResultRef->{ErrorMessage} = $Status->getAttribute("ErrorMessage");
		}
	};
	if ($@) {
		$$AResultRef->{Error} = 1; # true
		$$AResultRef->{ErrorMessage} = "Client Error: Cannot parse error";
	}
	return if ($$AResultRef->{Error} == 1);

	# RowCount/FieldCount
	eval {
		my $Status = $root->getChildrenByTagName("Status")->[0];
		$$AResultRef->{RowCount} = $Status->getAttribute("RowCount")+0;
		$$AResultRef->{FieldCount} = $Status->getAttribute("FieldCount")+0;
	};
	if ($@) {
		$$AResultRef->{Error} = 1;
		$$AResultRef->{ErrorMessage} = "Client Error: Cannot parse row/field count";
	}
	return if ($$AResultRef->{Error} == 1);

	# Field Names
	eval {
		my $Fields = $root->getChildrenByTagName("Fields")->[0];
		my @FieldArr = $Fields->getChildrenByTagName("Col");
		for my $Field (@FieldArr) {
			push(@{$$AResultRef->{Names}}, $Field->getAttribute("Name"));
		}
	};
	if ($@) {
		$$AResultRef->{Error} = 1;
		$$AResultRef->{ErrorMessage} = "Client Error: Cannot parse row/field count";
	}
	return if ($$AResultRef->{Error}) == 1;
			
	# Value/Type Names
	eval {
		my $Rows = $root->getChildrenByTagName("Rows")->[0];
		my @RowArr = $Rows->getChildrenByTagName("Row");
		my $r = -1;
		for my $Row (@RowArr) {
			$r += 1;
			my $c = -1;
			my @ColArr = $Row->getChildrenByTagName("Col");
			for my $Col (@ColArr) {
				$c += 1;
				$$AResultRef->{Type}[$r][$c] = $Col->getAttribute("Type");
				$$AResultRef->{Value}[$r][$c] = $Col->getAttribute("Value");
			}
		}
	};
	if ($@) {
		$$AResultRef->{Error} = 1;
		$$AResultRef->{ErrorMessage} = "Client Error: Cannot parse row/field count";
	}
	return if ($$AResultRef->{Error} == 1);
};

# Public: Constructor
sub new {
	my $class = shift;
	my $args  = { @_ };
	my $self = bless({}, $class);

	if ($args->{host} ne "") {	$ConnectionSettings->{host} = $args->{host};	}
	if ($args->{port} > 0) {	$ConnectionSettings->{port} = $args->{port};	}
	if ($args->{user} ne "") {	$ConnectionSettings->{user} = $args->{user};	}
	if ($args->{pass} ne "") {	$ConnectionSettings->{pass} = $args->{pass};	}
	
	return $self;
}

# Public: Execute SQL Query
sub ExecSQL {
	my $self = shift;
	my $ASQLQuery = shift;
	my $ANoResult = shift;

	# Init result
	my $Result = {
		XML		=> XML::LibXML::Document->new("1.0", "UTF-8"),
		Error		=> 1, # 1 = true, 0 = false
		ErrorMessage	=> "Result set is init value",
		RowCount	=> 0,
		FieldCount	=> 0,
		Names		=> [],
		Value		=> ([],[]),
		Type		=> ([],[])
	};

	$Result->{XML} = $Send->($self, $ASQLQuery, $ANoResult);
	if ($ANoResult == 1) { # 1 = true, 0 = false
		$Result->Error = 0;
		$Result->ErrorMessage = "";
	} else {
		$ParseSQLiteResult->($self, \$Result);
	}

	return $Result;
}

1;

=head

Perl Example:

#!/usr/bin/perl

use lib ".";
use SQLiteServerConnector;

my $SQLiteServerConnector = new SQLiteServerConnector(
        host => "127.0.0.1",	
        port => 11833,
        user => "Admin",
        pass => "Admin"
);

$Result = $SQLiteServerConnector->ExecSQL(
        "SELECT 1;",	// SQL Query
        false		// false = Request result
);

if ($Result->{Error}) {
        print "ERROR: $Result->{ErrorMessage}\n";
} else {
        print "Rows: $Result->{RowCount}\n";
        print "Fields: $Result->{FieldCount}\n";
        for my $i (0..$Result->{FieldCount}) {
                print "  * Field: $Result->{Names}[$i]\n";
        }
        for $r (0..$Result->{RowCount}-1) {
                print "Row: $r\n";
                for $c (0..$Result->{FieldCount}-1) {
                        print   "  * Row: $r " .
                                "Col: $c " .
                                "Name: " . $Result->{Names}[$c] . " " .
                                "Type: " . ($Result->{Type}[$r][$c]) . " " .
                                "Value: " . ($Result->{Value}[$r][$c]) . "\n";
                }
        }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                        
=cut
