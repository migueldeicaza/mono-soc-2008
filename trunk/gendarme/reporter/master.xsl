<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" encoding="utf-8"/>
	<xsl:template match="/">
		<xsl:for-each select="gendarme-output">
		<html>
			<head>
				<title>Gendarme Master Index</title>
			</head>
			<style type="text/css">
				h1, h2, h3 {
					font-family: Verdana;
					color: #68892F;
				}
				h2 {
					font-size: 14 pt;
				}
			</style>

			<body>
				<h1>Gendarme Master Page Report</h1>
				<p>Produced on <xsl:value-of select="@date"/> UTC.</p>
				<h2>Table of defects found:</h2>
				<table border="1">
				<th>Assembly</th>
				<th>Critical</th>
				<th>High</th>
				<th>Medium</th>
				<th>Low</th>
				<xsl:for-each select="assemblies/assembly">
					<tr>
					<a href="{@shortname}.xml">
						<xsl:value-of select="@shortname"/>
					</a>
						<td>
							<a href="{@shortname}.Critical.xml">
							<xsl:value-of select="@critical"/>
							</a>
						</td>
						<td>
							<a href="{@shortname}.High.xml">
							<xsl:value-of select="@high"/>
							</a>
						</td>
						<td>
							<a href="{@shortname}.Medium.xml">
							<xsl:value-of select="@medium"/>
							</a>
						</td>
						<td>
							<a href="{@shortname}.Medium.xml">
							<xsl:value-of select="@low"/>
							</a>
						</td>
					</tr>
				</xsl:for-each>
				</table>
			</body>
		</html>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
